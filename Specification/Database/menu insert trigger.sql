create trigger yummyonlinedb_menu_insert
on Menus
	for insert
as
	declare @id int, @hotelid int, @name nvarchar(20);
	select @id = id, @name = name from inserted;
	select top 1 @hotelid = id from HotelConfigs;
	insert into YummyOnlineDB.dbo.MenuGathers values(@id,@hotelid,@name);
go

create trigger yummyonlinedb_menu_delete
on Menus
	for delete
as
	declare @id int, @hotelid int;
	select @id = id from deleted;
	select top 1 @hotelid = id from HotelConfigs;
	delete from YummyOnlineDB.dbo.MenuGathers where Id = @id and HotelId = @hotelid;
go