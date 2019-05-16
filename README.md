# Building the Bangazon Platform API

Welcome, new Bangazonians!

Your job is to build out a .NET Web API that makes each resource in the Bangazon ERD available to application developers throughout the entire company.

1. Products
1. Product types
1. Customers
1. Orders
1. Payment types
1. Employees
1. Computers
1. Training programs
1. Departments

> **Pro tip:** You do not need to make a Controller for the join tables, because those aren't resources.

Your product owner will provide you with a prioritized backlog of features for you to work on over the development sprint. The first version of the API will be completely open since we have not determined which authentication method we want to use yet.

The only restriction on the API is that only requests from the `www.bangazon.com` domain should be allowed. Requests from that domain should be able to access every resource, and perform any operation a resource.

## Plan

First, you need to plan. Your team needs to come to a consensus about the Bangazon ERD design. Once you feel you have consensus, you must get it approved by your manager before you begin writing code for the API.

## Modeling

Next, you need to author the Models needed for your API. Make sure that each model has the approprate foreign key relationship defined on it, either with a custom type or an `List<T>` to store many related things. The boilerplate code shows you one example - the relationship between `Order` and `OrderProduct`, which is 1 -> &#8734;. For every _OrderId_, it can be stored in the `OrderProduct` table many times.

## Database Management

You will be using the [Official Bangazon SQL](./bangazon.sql) file to create your database. Create the database using SSMS, create a new SQL script for that database, copy the contents of the SQL file into your script, and then execute it.

## Controllers

Now it's time to build the controllers that handle GET, POST, PUT, and DELETE operations on each resource. Make sure you read, and understand, the requirements in the issue tickets to you can use  SQL to return the correct data structure to client requests.

## Test Classes

Each feature ticket your team will work on for this sprint has testing requirements. This boilerplate solution has a testing project includes with some starter code. You must make sure that all tests pass before you submit a PR.

## Directions For Use

1. Make sure the nuGet packages **System.Data.SqlClient**, **xunit**, and **xunit.runner.visualstudio** are installed in Microsoft Visual Studio
1. Download the code and open in Visual Studio
1. Run the code (Debug menu > Start without Debugging)
1. Congratulations, the Bangazon API server is now running!

Ensure your database is properly created using the [Official Bangazon SQL](./create-bangazonapi-tables.sql) and fill with desired data. This can be accomplished using either a SQL database program such as Azure Data Studio, or an API front-end program such as Postman.

In order to enable built-in integrated tests, please use the following SQL code: [Official Bangazon Testing Data](./fill-bangazon-tables.sql) in Azure Data Studio.

If any tests fail, re-run CREATE, then FILL, then run that test by itself. If it fails again, please report the results as a bug.
