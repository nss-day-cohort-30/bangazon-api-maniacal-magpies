INSERT INTO Customer (FirstName, LastName) VALUES ('Summer', 'Rainault');
INSERT INTO Customer (FirstName, LastName) VALUES ('Kieran', 'Dubhglas');
INSERT INTO Customer (FirstName, LastName) VALUES ('Barry', 'Allen');
INSERT INTO Customer (FirstName, LastName) VALUES ('Antonio', 'Jefferson');

INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) 
	SELECT '3504', 'Discover', c.Id FROM Customer c WHERE c.FirstName = 'Antonio';
INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) 
	SELECT '12345','Visa', c.Id FROM Customer c WHERE c.FirstName = 'Summer';
INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) 
	SELECT '87234023','Pinnacle', c.Id FROM Customer c WHERE c.FirstName = 'Summer';
INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) 
	SELECT '4526323','Visa', c.Id FROM Customer c WHERE c.FirstName = 'Kieran';
INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) 
	SELECT '340926983','MasterCard', c.Id FROM Customer c WHERE c.FirstName = 'Kieran';
INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) 
	SELECT '378237328','discovercard', c.Id FROM Customer c WHERE c.FirstName = 'Barry';
INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) 
	SELECT '347239392','bankaccount', c.Id FROM Customer c WHERE c.FirstName = 'Barry';

INSERT INTO ProductType ([Name]) VALUES ('Jewellery');
INSERT INTO ProductType ([Name]) VALUES ('Stuffed Toy');
INSERT INTO ProductType ([Name]) VALUES ('Collectible');
INSERT INTO ProductType ([Name]) VALUES ('Used Book');

INSERT INTO Product (Price, Title, [Description], Quantity, ProductTypeId, CustomerId)
	SELECT '11.50', 'Electronic Thingy', 'It''s a thing and it''s electronic', 5,
	pt.Id, c.Id FROM ProductType pt, Customer c
	WHERE pt.Name = 'Jewellery' AND c.FirstName = 'Summer';
INSERT INTO Product (Price, Title, [Description], Quantity, ProductTypeId, CustomerId)
	SELECT 1234, 'Love Baskets', 'Awww', '2',
	pt.Id, c.Id FROM ProductType pt, Customer c
	WHERE pt.Name = 'Jewellery' AND c.FirstName = 'Antonio';
INSERT INTO Product (Price, Title, [Description], Quantity, ProductTypeId, CustomerId)
	SELECT '10.75','Barry Allen Pop','still in box, target exclusive','3',
	pt.Id, c.Id FROM ProductType pt, Customer c
	WHERE pt.Name = 'Collectible' AND c.FirstName = 'Summer';
INSERT INTO Product (Price, Title, [Description], Quantity, ProductTypeId, CustomerId)
	SELECT '200','linked gemstone necklace','handmade','1',
	pt.Id, c.Id FROM ProductType pt, Customer c
	WHERE pt.Name = 'Jewellery' AND c.FirstName = 'Summer';
INSERT INTO Product (Price, Title, [Description], Quantity, ProductTypeId, CustomerId)
	SELECT '10.75','Jughead Pop','still in box, hot topic exclusive','1',
	pt.Id, c.Id FROM ProductType pt, Customer c
	WHERE pt.Name = 'Collectible' AND c.FirstName = 'Kieran';

INSERT INTO [Order] (CustomerId, PaymentTypeId)
	SELECT c.Id, pt.Id FROM Customer c, PaymentType pt
	WHERE c.FirstName = 'Summer' AND pt.Name = 'Pinnacle';

INSERT INTO OrderProduct (OrderId, ProductId)
	SELECT 1, p.Id FROM Product p WHERE p.Title = 'Jughead Pop';

INSERT INTO Computer (Make, Manufacturer, PurchaseDate)
	VALUES ('VivoBook', 'ASUS', '08-17-2018');
INSERT INTO Computer (Make, Manufacturer, PurchaseDate)
	VALUES ('Inspiron', 'Dell', '01-05-2018');
INSERT INTO Computer (Make, Manufacturer, PurchaseDate)
	VALUES ('PowerBook', 'IBM', '01-05-2018');

INSERT INTO Department ([Name], Budget)
	VALUES ('IT',300000);
INSERT INTO Department ([Name], Budget)
	VALUES ('Shipping',600000);
INSERT INTO Department ([Name], Budget)
	VALUES ('Billing',650000);

INSERT INTO Employee (FirstName, LastName, DepartmentId)
	SELECT 'Leonard','Snart', d.Id FROM Department d
	WHERE d.Name = 'IT';
INSERT INTO Employee (FirstName, LastName, DepartmentId)
	SELECT 'Winter','Rainault', d.Id FROM Department d
	WHERE d.Name = 'IT';
INSERT INTO Employee (FirstName, LastName, DepartmentId)
	SELECT 'Iris','West', d.Id FROM Department d
	WHERE d.Name = 'Billing';

INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate)
	SELECT e.Id , c.Id, GETDATE() FROM Employee e, Computer c
	WHERE e.FirstName = 'Leonard' AND c.Make = 'VivoBook';

INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate)
	SELECT e.Id , c.Id, GETDATE() FROM Employee e, Computer c
	WHERE e.FirstName = 'Iris' AND c.Make = 'Inspiron';

INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees)
VALUES ('Cyber Awareness', '05-20-2019', '05-21-2019', '20');
INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees)
VALUES ('Windows for Mac Users', '06-01-2019', '07-03-2019', '10');
INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees)
VALUES ('Personal Hygiene for Programmers', '08-10-2019', '08-12-2019', '15');
INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees)
VALUES ('Equity: As good as money!', '02-25-2019', '02-27-2019', '10');

INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
VALUES (1, 1);
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
VALUES (1, 2);
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
VALUES (1, 3);
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
VALUES (2, 1);
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
VALUES (2, 2);
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
VALUES (3, 1);
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
<<<<<<< HEAD
VALUES (3, 1);
=======
VALUES (3, 1); 
>>>>>>> master
