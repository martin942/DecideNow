--declare @databaseName sysname;
--set @databaseName = 'DecideDBv1';

--if db_id(@databaseName) is not null
--	BEGIN
--	   PRINT 'Database already exists';
--	END
--ELSE
--	BEGIN
--		create database DecideDBv1;	
--		Print 'Database created'
--	END;
--GO


use DecideDBv1;

if object_id(N'dbo.[user]', N'U') IS NOT NULL  
   drop table dbo.[user]; 

create table [user] (
	id varchar(36) primary key,
	username nvarchar(30),
	firstname nvarchar(30),
	lastname nvarchar(30),
	email nvarchar(320),
	birthdate varchar(10),
	passwordhash varchar(64)
);
