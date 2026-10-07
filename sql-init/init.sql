-- إنشاء قاعدة البيانات إذا لم تكن موجودة
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TicketingDB')
BEGIN
    CREATE DATABASE [TicketingDB];
END
GO

USE [TicketingDB];
GO

-- ملاحظة: الميغريشنز ستُنشئ الجداول عبر EF Core
-- هذا الملف يحتوي فقط على بيانات أولية بعد تطبيق الميغريشنز
-- يُنفَّذ يدويًا أو عبر Migration إضافية
