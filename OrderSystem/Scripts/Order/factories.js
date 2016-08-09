app.factory('generateDataPromise', [
	'$http',
	'$q',
	'dataSet',
	function ($http, $q, $dataSet) {
		function _filterMenuClass(menuClass) {
			menuClass.Addition = {
				IsSelected: false,
				Ordered: 0,
			};
			return menuClass;
		}
		function _filterMenu(menu) {
			if (menu.MenuPrice == null) {
				menu.MenuPrice = {
					Price: 0,
					Discount: 1,
				}
			}
			menu.Addition = {
				Ordered: 0,
				Remarks: [],
				OrderedSetMealClasses: [],

				OriPrice: menu.MenuPrice.Price,

				IsOnSale: false,
				ExcludePayDiscount: false,
				IsShowRemark: menu.Remarks != null && menu.Remarks.length > 0,
				IsRemarkCollapsed: false,
				IsSetMealCollapsed: false,
			};
			menu.MenuPrice.Price = menu.MenuPrice.Price * menu.MenuPrice.Discount;

			angular.extend(menu, {
				IsShowSetMeal: function () {
					return this.IsSetMeal;
				},
				ToggleSetMeal: function () {
					this.Addition.IsSetMealCollapsed = !this.Addition.IsSetMealCollapsed;
				},

				IsShowRemark: function () {
					return this.Addition.Ordered > 0 && this.Addition.IsShowRemark;
				},
				IsRemarkCollapsed: function () {
					return this.Addition.IsRemarkCollapsed && this.Remarks.length > 0;
				},
				ToggleRemark: function () {
					this.Addition.IsRemarkCollapsed = !this.Addition.IsRemarkCollapsed;
				}
			});

			return menu;
		}

		function _filterSetMeal(setMeal, classes) {
			setMeal.Addition.SetMeal = {
				Classes: classes
			};

			for (var j in setMeal.Addition.SetMeal.Classes) {
				var setMealClass = setMeal.Addition.SetMeal.Classes[j];
				setMealClass.Addition = {
					IsSelected: false,
					Ordered: 0,
					OrderedMenus: []
				};

				for (var k in setMealClass.Menus) {
					var menu = setMealClass.Menus[k];
					menu.Addition = {
						Ordered: 0,
					};
				}
			}
		}

		return function () {
			return $q(function (resolve) {
				var menuOnSales, menuSetMeals, payKinds;

				$http.post('/Order/GetMenuInfos').then(function (response) {
					var data = response.data;

					$dataSet.MenuClasses = data.MenuClasses;

					$dataSet.Menus = data.Menus;

					menuOnSales = data.MenuOnSales;
					$dataSet.MenuOnSalesCount = menuOnSales.length;
					menuSetMeals = data.MenuSetMeals;

					payKinds = data.PayKinds;
					$dataSet.DiscountMethods = data.DiscountMethods;
					$dataSet.Hotel = data.Hotel;

					loadFinished();
				});
				function loadFinished() {
					for (var i in payKinds) {
						if (payKinds[i].Type == 3) {
							$dataSet.PayKinds.push(payKinds[i]);
						} else {
							$dataSet.PayKinds.splice(0, 0, payKinds[i]);
						}
					}


					for (var i in $dataSet.MenuClasses) {
						_filterMenuClass($dataSet.MenuClasses[i]);
					}

					for (var i in $dataSet.Menus) {
						var menu = $dataSet.Menus[i];
						_filterMenu(menu)

						if (menu.MenuPrice.Discount < 1 || menu.MenuPrice.ExcludePayDiscount) {
							setExcludePayKindDiscount(menu);
						}
						for (var j in menuOnSales) {
							if (menuOnSales[j].Id == menu.Id) {
								menu.Addition.IsOnSale = true;
								menu.MenuPrice.Price = menuOnSales[j].Price;
								menu.MenuPrice.Discount = 1;
								setExcludePayKindDiscount(menu);
								break;
							}
						}
						for (var j in menuSetMeals) {
							if (menuSetMeals[j].SetMealId == menu.Id && menu.IsSetMeal) {
								_filterSetMeal(menu, menuSetMeals[j].Classes);

								setExcludePayKindDiscount(menu);
							}
						}
					}
					resolve();
				}
				// 该菜品不算入全单打折中
				function setExcludePayKindDiscount(menu) {
					menu.Addition.ExcludePayDiscount = true;
				}
			});
		}
	}]);

app.factory('dataSet', [
	'$rootScope',
	function ($rootScope) {
		var dataSet = {
			Hotel: null,
			MenuClasses: [], // 菜单分类
			Menus: [], //所有菜品
			PayKinds: [],
			DiscountMethods: null,
			MenuOnSalesCount: 0,
			Reset: function () {
				for (var i in this.MenuClasses) {
					this.MenuClasses[i].Addition.Ordered = 0;
				}
				for (var i in this.Menus) {
					this.Menus[i].Addition.Ordered = 0;
					this.Menus[i].Addition.IsRemarkCollapsed = false;
					this.Menus[i].Addition.IsSetMealCollapsed = false;
					this.Menus[i].Remarks = this.Menus[i].Remarks.concat(this.Menus[i].Addition.Remarks);
					this.Menus[i].Addition.Remarks = [];
				}
			}
		}
		$rootScope.dataSet = dataSet;
		return dataSet;
	}]
);

app.factory('cart', [
	'$rootScope',
	'$q',
	'$http',
	'dataSet',
	'$localStorage',
	'generateDataPromise',
	function ($rootScope, $q, $http, $dataSet, $localStorage, generateData) {
		var cart = {
			/* Cart Data */
			HeadCount: 1, // 总人数
			Price: 0, // 总价
			PriceInPoints: 0, // 通过积分支付的价格
			Invoice: null, // 发票抬头
			Desk: null,
			PayKind: null,
			OrderedMenus: [],
			/* Cart Data */

			IsInitialized: false, // 是否初始化
			DiscountMethod: {
				Discount: 1,
				Name: ''
			},
			Customer: null,
			Ordered: 0, // 总共已选的份数

			Reset: function () {
				this.Ordered = 0;
				this.OriPrice = 0;
				this.Price = 0;
				this.OrderedMenus = [];
				$dataSet.Reset();
				delete $localStorage.cart;
			},
			Initialize: function (resolve) {
				if (this.IsInitialized) {
					if (resolve != null) resolve();
				} else {
					var _this = this;
					$rootScope.isLoading = true;
					generateData().then(function () {
						if (!angular.isUndefined($localStorage.cart)) {
							_this.LoadExistedCart($localStorage.cart);
						}
						$localStorage.cart = _this;
						$http.post('/Order/GetCurrentDesk').then(function (response) {
							_this.Desk = response.data;
						});
						if (resolve != null) resolve();
						_this.IsInitialized = true;
						$rootScope.isLoading = false;
					});
				}
			},
			_loadSetMeal: function (setMeal, existedSetMealClasses) {
				for (var i in setMeal.Addition.OrderedSetMealClasses[setMeal.Addition.OrderedSetMealClasses.length - 1]) {
					var setMealClass = setMeal.Addition.OrderedSetMealClasses[setMeal.Addition.OrderedSetMealClasses.length - 1][i];
					if (typeof setMealClass == 'string')
						continue;
					for (var j in existedSetMealClasses) {
						var existedSetMealClass = existedSetMealClasses[j];
						if (setMealClass.Id == existedSetMealClass.Id) {
							for (var k in setMealClass.Menus) {
								var setMealMenu = setMealClass.Menus[k];

								for (var l in existedSetMealClass.Addition.OrderedMenus) {
									var existedSetMealMenu = existedSetMealClass.Addition.OrderedMenus[l];
									if (setMealMenu.Menu.Id == existedSetMealMenu.Menu.Id) {

										for (var m = 0; m < existedSetMealMenu.Addition.Ordered / setMealMenu.Count ; m++) {
											this.AddSetMealMenu(setMealMenu, setMealClass);
										}
										console.log(setMeal)
									}
								}
							}
						}

					}
				}


			},
			LoadExistedCart: function (existedCart) {
				this.HeadCount = existedCart.HeadCount;
				this.Invoice = existedCart.Invoice;

				for (var i in existedCart.OrderedMenus) {
					for (var j in $dataSet.Menus) {
						if (existedCart.OrderedMenus[i].Id == $dataSet.Menus[j].Id) {
							var existedMenu = existedCart.OrderedMenus[i];
							var menu = $dataSet.Menus[j];

							var count = existedMenu.Addition.Ordered - menu.MinOrderCount + 1;

							for (var k = 0; k < count; k++) {
								this.AddMenu(menu);
								if (menu.IsSetMeal) {
									this._loadSetMeal(menu, existedMenu.Addition.OrderedSetMealClasses[k]);
								}
							};

							for (var k in existedCart.OrderedMenus[i].Addition.Remarks) {
								for (var l in menu.Remarks) {
									if (existedCart.OrderedMenus[i].Addition.Remarks[k].Id == menu.Remarks[l].Id) {
										this.AddRemark(menu, menu.Remarks[l], l);
										break;
									}
								}
							}
							break;
						}
					}
				}

				this.IsInitialized = true;
			},

			_menuPriceHandler: function (menu, ordered) {
				this.Price += menu.MenuPrice.Price * ordered;
			},
			AddMenu: function (menu) {
				var min = 1;
				if (menu.Addition.Ordered == 0) {
					min = menu.MinOrderCount;
				}
				menu.Addition.Ordered += min;
				this.Ordered += min;
				this._menuPriceHandler(menu, min);
				if (menu.Addition.Ordered == menu.MinOrderCount) {
					this.OrderedMenus.push(menu);
				}
				if (menu.IsSetMeal) {
					menu.Addition.OrderedSetMealClasses.push(angular.copy(menu.Addition.SetMeal.Classes));
				}

				for (var i in $dataSet.MenuClasses) {
					for (var j in menu.MenuClasses) {
						if ($dataSet.MenuClasses[i].Id == menu.MenuClasses[j]) {
							$dataSet.MenuClasses[i].Addition.Ordered += min;
							break;
						}
					}
				}
			},
			RemoveMenu: function (menu) {
				var min = 1;
				if (menu.Addition.Ordered == menu.MinOrderCount) {
					min = menu.MinOrderCount;
				}
				menu.Addition.Ordered -= min;
				this.Ordered -= min;
				this._menuPriceHandler(menu, -min);
				if (menu.Addition.Ordered == 0) {
					for (var i = 0; i < this.OrderedMenus.length; i++) {
						if (angular.equals(menu, this.OrderedMenus[i])) {
							this.OrderedMenus.splice(i, 1);
							break;
						}
					}
				}
				if (menu.IsSetMeal) {
					menu.Addition.OrderedSetMealClasses.splice(menu.Addition.OrderedSetMealClasses.length - 1, 1);
				}

				for (var i in $dataSet.MenuClasses) {
					for (var j in menu.MenuClasses) {
						if ($dataSet.MenuClasses[i].Id == menu.MenuClasses[j]) {
							$dataSet.MenuClasses[i].Addition.Ordered -= min;
							break;
						}
					}
				}
			},
			AddSetMealMenu: function (setMealMenu, setMealClass) {
				var min = setMealMenu.Count;
				setMealMenu.Addition.Ordered += min;
				setMealClass.Addition.Ordered += min;

				if (setMealMenu.Addition.Ordered == min) {
					setMealClass.Addition.OrderedMenus.push(setMealMenu);
				}
			},
			RemoveSetMealMenu: function (setMealMenu, setMealClass) {
				var min = setMealMenu.Count;
				setMealMenu.Addition.Ordered -= min;
				setMealClass.Addition.Ordered -= min;

				if (setMealMenu.Addition.Ordered == 0) {
					for (var i = 0; i < setMealClass.Addition.OrderedMenus.length; i++) {
						if (angular.equals(setMealMenu, setMealClass.Addition.OrderedMenus[i])) {
							setMealClass.Addition.OrderedMenus.splice(i, 1);
							break;
						}
					}
				}
			},
			AddRemark: function (menu, remark, index) {
				menu.Addition.Remarks.push(remark);
				menu.Remarks.splice(index, 1);
				this.Price += remark.Price;
			},
			RemoveRemark: function (menu, remark, index) {
				menu.Remarks.push(remark);
				menu.Addition.Remarks.splice(index, 1);
				this.Price -= remark.Price;
			},

			GetOrderedMenusWithFixed: function () {
				var filteredArr = [];
				for (var i in $dataSet.Menus) {
					if ($dataSet.Menus[i].IsFixed) {
						filteredArr.push($dataSet.Menus[i]);
					}
				}
				for (var i in this.OrderedMenus) {
					if (!this.OrderedMenus[i].IsFixed) {
						filteredArr.push(this.OrderedMenus[i]);
					}
				}
				return filteredArr
			},

			// 获得最优的整单折扣方案
			GetMinDiscountMethod: function () {
				var minDiscountMethod = {
					Discount: 1,
					Name: ''
				};
				if (!this.IsInitialized)
					return minDiscountMethod;

				if (this.PayKind != null) {
					if (this.PayKind.Discount < minDiscountMethod.Discount) {
						minDiscountMethod.Discount = this.PayKind.Discount;
						minDiscountMethod.Name = this.PayKind.Name + '折扣';
					}
				}
				var now = new Date();
				now = now.toTimeString();
				for (var i in $dataSet.DiscountMethods.TimeDiscounts) {
					var timeDiscount = $dataSet.DiscountMethods.TimeDiscounts[i];
					var from = new Date('1/1/1 ' + timeDiscount.From);
					var to = new Date('1/1/1 ' + timeDiscount.To);
					from = from.toTimeString();
					to = to.toTimeString();
					if (now >= from && now <= to) {
						if (timeDiscount.Discount < minDiscountMethod.Discount) {
							minDiscountMethod.Discount = timeDiscount.Discount;
							minDiscountMethod.Name = timeDiscount.Name;
						}
						break;
					}
				}

				if (this.Customer != null) {
					for (var i in $dataSet.DiscountMethods.VipDiscounts) {
						var vipDiscount = $dataSet.DiscountMethods.VipDiscounts[i];
						if (vipDiscount.Id == this.Customer.VipLevelId) {
							if (vipDiscount.Discount < minDiscountMethod.Discount) {
								minDiscountMethod.Discount = vipDiscount.Discount;
								minDiscountMethod.Name = vipDiscount.Name;
							}
							break;
						}
					}
				}
				this.DiscountMethod = minDiscountMethod;
				return minDiscountMethod;
			},

			// 获得最终要支付的金额
			GetPaymentPrice: function () {
				if (this.PayKind == null) {
					return null;
				}
				var price = 0;
				var discountMethod = this.GetMinDiscountMethod();
				for (var i in this.OrderedMenus) {
					var menuPrice = this.OrderedMenus[i].MenuPrice.Price * this.OrderedMenus[i].Addition.Ordered;
					if (!this.OrderedMenus[i].Addition.ExcludePayDiscount) { // 如果菜品在打支付总折的范围内，则打折
						menuPrice *= discountMethod.Discount;
					}
					price += menuPrice;

					for (var j in this.OrderedMenus[i].Addition.Remarks) {
						price += this.OrderedMenus[i].Addition.Remarks[j].Price;
					}
				}
				price = parseFloat(price.toFixed(2));
				if (this.PayKind.Type != 0) {
					this.PriceInPoints = 0;
				}
				// 如果用户抵扣的金额超过需要支付的金额
				if (this.PriceInPoints > price) {
					this.PriceInPoints = price;
				}
				// 总价减去积分抵扣的价格
				price -= this.PriceInPoints;
				return price;
			},
			CanSubmit: function () {
				return this.Price > 0 &&
					this.Price - this.PriceInPoints >= 0 && this.PriceInPoints >= 0
			},
			_getSendData: function () {
				var sendData = {
					HeadCount: this.HeadCount,
					Price: this.GetPaymentPrice() + this.PriceInPoints,
					PriceInPoints: this.PriceInPoints,
					Invoice: this.Invoice,

					DeskId: this.Desk.Id,
					PayKindId: this.PayKind.Id,

					OrderedMenus: []
				}

				for (var i in this.OrderedMenus) {
					var menu = {
						Id: this.OrderedMenus[i].Id,
						Ordered: this.OrderedMenus[i].Addition.Ordered,
						Remarks: []
					}
					for (var j in this.OrderedMenus[i].Addition.Remarks) {
						menu.Remarks.push(this.OrderedMenus[i].Addition.Remarks[j].Id);
					}

					if (this.OrderedMenus[i].IsSetMeal) {
						console.log(this.OrderedMenus[i])
						for (var j in this.OrderedMenus[i].Addition.OrderedSetMealClasses) {
							var orderedSetMealClasses = this.OrderedMenus[i].Addition.OrderedSetMealClasses[j];

							var setMeal = {
								Id: menu.Id,
								Ordered: 1,
								Remarks: menu.Remarks,
								SetMealClasses: []
							}

							for (var j in orderedSetMealClasses) {
								var orderedSetMealClass = orderedSetMealClasses[j];
								if (typeof orderedSetMealClass == 'string')
									continue;
								var setMealClass = {
									Id: orderedSetMealClass.Id,
									OrderedMenus: []
								}
								for (var k in orderedSetMealClass.Addition.OrderedMenus) {
									var orderedSetMealMenu = orderedSetMealClass.Addition.OrderedMenus[k];

									setMealClass.OrderedMenus.push({
										Id: orderedSetMealMenu.Menu.Id,
										Ordered: orderedSetMealMenu.Addition.Ordered
									})
								}
								setMeal.SetMealClasses.push(setMealClass);
							}

							sendData.OrderedMenus.push(setMeal);
						}
					} else {
						sendData.OrderedMenus.push(menu);
					}
				}
				console.log(sendData)
				return sendData;
			},
			Submit: function () {
				var _this = this;
				return $q(function (resolve, reject) {
					var sendData = _this._getSendData();
					$http.post('/Payment/Pay', sendData).then(function (response) {
						resolve(response.data);
					}, function () {
						reject();
					});
				});
			}
		}
		$rootScope.cart = cart;
		return cart;
	}]
).factory('menuFilter', [
	'$rootScope',
	'$filter',
	'dataSet',
	function ($rootScope, $filter, $dataSet) {
		var mode = {
			search: 0,
			normal: 1,
			rank: 2,
			onSale: 3,
			result: 4
		}
		var currentMode = mode.normal;

		var menuFilter = {
			FilteredMenus: [],
			SearchText: '',
			_activeMenuClass: null,

			IsRankMode: function () {
				return currentMode == mode.rank;
			},
			IsSearchMode: function () {
				return currentMode == mode.search;
			},
			IsOnSaleMode: function () {
				return currentMode == mode.onSale;
			},
			IsResultMode: function () {
				return currentMode == mode.result;
			},
			_changeMode: function (m) {
				currentMode = m;
				if (m != mode.search) {
					this.SearchText = '';
				}
				if (m != mode.normal && this._activeMenuClass != null) {
					this._activeMenuClass.Addition.IsSelected = false;
				}
			},
			IntoRankMode: function () {
				this._changeMode(mode.rank);
				this._filterMenu();
			},
			IntoOnSaleMode: function () {
				this._changeMode(mode.onSale);
				this._filterMenu();
			},
			IntoResultMode: function () {
				this._changeMode(mode.result);
				this._filterMenu();
			},
			ToggleSelected: function (menuClass) {
				this._changeMode(mode.normal);

				if (this._activeMenuClass != null) {
					this._activeMenuClass.Addition.IsSelected = false;
				}

				menuClass.Addition.IsSelected = true;
				this._activeMenuClass = menuClass;
				this._filterMenu();
			},
			ChangeSearchText: function () {
				if (this.SearchText == '') {
					this._changeMode(mode.rank);
				} else {
					this._changeMode(mode.search);
				}
				this._filterMenu();
			},
			_filterMenu: function () {
				var filteredArr = [];

				switch (currentMode) {
					case mode.normal:
						filteredArr = $filter('filter')($dataSet.Menus, {
							MenuClasses: this._activeMenuClass.Id
						}, true);
						filteredArr = $filter('orderBy')(filteredArr, '-Ordered');
						break;
					case mode.rank:
						filteredArr = $filter('orderBy')($dataSet.Menus, '-Ordered');
						filteredArr = $filter('limitTo')(filteredArr, 15);
						break;
					case mode.search:
						filteredArr = $filter('filter')($dataSet.Menus, {
							Code: this.SearchText
						});
						filteredArr = $filter('limitTo')(filteredArr, 15);
						break;
					case mode.onSale:
						for (var i in $dataSet.Menus) {
							if ($dataSet.Menus[i].Addition.IsOnSale) {
								filteredArr.push($dataSet.Menus[i]);
							}
						}
						break;
				}

				this.FilteredMenus = filteredArr;
			}
		}

		$rootScope.menuFilter = menuFilter;
		return menuFilter;
	}]
);

app.factory('setMealFilter', [
	'$rootScope',
	'$filter',
	'cart',
	'dataSet',
	function ($rootScope, $filter, $cart, $dataSet) {
		var setMealFilter = {
			ActiveClasses: null,
			ActiveClass: null,

			FilteredSetMealClasses: [],

			IsShowSetMeal: function (setMeal) {
				return setMeal.IsSetMeal && setMeal.Addition.Ordered > 0;
			},
			CanClassAddMenu: function (setMealMenu, setMealClass) {
				return setMealMenu.Count + setMealClass.Addition.Ordered <= setMealClass.Count;
			},
			CanCloseSetMealModal: function () {
				var ordered = 0;
				for (var i in this.ActiveClasses) {
					if (typeof this.ActiveClasses[i] == 'string')
						continue;
					var setMealClass = this.ActiveClasses[i];
					ordered += setMealClass.Addition.Ordered;
				}
				return ordered != 0;
			},
			IsSetMealAllOrdered: function () {
				for (var i in $cart.OrderedMenus) {
					var setMeal = $cart.OrderedMenus[i];
					if (!setMeal.IsSetMeal)
						continue;

					console.log(setMeal.Addition.OrderedSetMealClasses);
					for (var j in setMeal.Addition.OrderedSetMealClasses) {
						var setMealClasses = setMeal.Addition.OrderedSetMealClasses[j];

						for (var k in setMealClasses) {
							var setMealClass = setMealClasses[k];
							if (typeof setMealClass == 'string') {
								continue;
							}
							if (setMealClass.Count != setMealClass.Addition.Ordered) {
								return false;
							}
						}
					}
				}
				return true;
			},
			ToggleSetMealSelected: function (setMealClasses) {
				this.ActiveClasses = setMealClasses;
				this.ToggleClassSelected(this.ActiveClasses[0]);

				this.FilteredSetMealClasses = setMealClasses;
			},
			ToggleClassSelected: function (setMealClass) {
				if (this.ActiveClass != null) {
					this.ActiveClass.Addition.IsSelected = false;
				}
				setMealClass.Addition.IsSelected = true;
				this.ActiveClass = setMealClass;
			},
		}

		$rootScope.setMealFilter = setMealFilter;
		return setMealFilter;
	}]
);

app.factory('dineToCart', [
	'cart',
	function ($cart) {
		return function (dine) {
			var cart = {
				HeadCount: dine.HeadCount,
				Invoice: dine.Invoice,
				OrderedMenus: [],
			}
			for (var i = 0; i < dine.DineMenus.length; i++) {
				var dineMenu = dine.DineMenus[i];

				var orderedMenu = {
					Id: dineMenu.Menu.Id,
					Addition: {
						Ordered: dineMenu.Count,
						Remarks: dineMenu.Remarks
					}
				};
				cart.OrderedMenus.push();
			}

			$cart.Reset();
			$cart.LoadExistedCart(cart);
		}
	}
]);