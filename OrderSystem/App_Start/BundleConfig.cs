using System.Web;
using System.Web.Optimization;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System;

namespace OrderSystem {
	public class BundleConfig {
		// 有关绑定的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=301862
		private class BundleInfo {
			public BundleInfo(string bundleName, string cdn) {
				BundleName = bundleName;
				Cdn = cdn;
			}
			public string BundleName { get; set; }
			public string Cdn { get; set; }
			public string LocalPath { get; set; }
		}
		public static void RegisterBundles(BundleCollection bundles) {

			bundles.UseCdn = true;
			BundleTable.EnableOptimizations = true;

			List<BundleInfo> styleInfos = new List<BundleInfo> {
				new BundleInfo("bootstrap","//cdn.bootcss.com/bootstrap/3.3.6/css/bootstrap.min.css"),
				new BundleInfo("fontawesome","//cdn.bootcss.com/font-awesome/4.5.0/css/font-awesome.min.css"),
				new BundleInfo("toastr","//cdn.bootcss.com/toastr.js/latest/css/toastr.min.css")
			};


			List<BundleInfo> scriptInfos = new List<BundleInfo> {
				new BundleInfo("jquery","//cdn.bootcss.com/jquery/2.2.1/jquery.min.js"),
				new BundleInfo("bootstrap","//cdn.bootcss.com/bootstrap/3.3.6/js/bootstrap.min.js"),
				new BundleInfo("angular","//cdn.bootcss.com/angular.js/1.5.7/angular.min.js"),
				new BundleInfo("angular-route","//cdn.bootcss.com/angular.js/1.5.7/angular-route.min.js"),
				new BundleInfo("angular-ui-bootstrap","//cdn.bootcss.com/angular-ui-bootstrap/1.2.1/ui-bootstrap-tpls.min.js"),

				new BundleInfo("toastr","//cdn.bootcss.com/toastr.js/latest/js/toastr.min.js")
			};

			foreach(BundleInfo s in styleInfos) {
				Bundle bundle = new StyleBundle($"~/css/{s.BundleName}", s.Cdn);
				if(s.LocalPath != null) {
					bundle = bundle.Include(s.LocalPath);
				}
				bundles.Add(bundle);
			}
			foreach(BundleInfo s in scriptInfos) {
				Bundle bundle = new ScriptBundle($"~/scripts/{s.BundleName}", s.Cdn);
				if(s.LocalPath != null) {
					bundle = bundle.Include(s.LocalPath);
				}
				bundles.Add(bundle);
			}
		}
	}
}
