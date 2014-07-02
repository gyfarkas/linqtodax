 
 
// Generated Code 
using LinqToDAX;
using System;
using System.Linq;
using TabularEntities;
namespace AdventureWorks
{
	

public interface IAdventureworks
{
 	 IQueryable<Currency> CurrencySet { get;  } 
	 IQueryable<Customer> CustomerSet { get;  } 
	 IQueryable<Date> DateSet { get;  } 
	 IQueryable<Employee> EmployeeSet { get;  } 
	 IQueryable<Geography> GeographySet { get;  } 
	 IQueryable<Product> ProductSet { get;  } 
	 IQueryable<ProductCategory> ProductCategorySet { get;  } 
	 IQueryable<ProductSubcategory> ProductSubcategorySet { get;  } 
	 IQueryable<Promotion> PromotionSet { get;  } 
	 IQueryable<Reseller> ResellerSet { get;  } 
	 IQueryable<SalesTerritory> SalesTerritorySet { get;  } 
	 IQueryable<InternetSales> InternetSalesSet { get;  } 
	 IQueryable<ProductInventory> ProductInventorySet { get;  } 
	 IQueryable<ResellerSales> ResellerSalesSet { get;  } 
	 IQueryable<SalesQuota> SalesQuotaSet { get;  } 
	 IQueryable<Numbers> NumbersSet { get;  } 
}


public class AdventureWorksContext : IAdventureworks
{
	
	public IQueryable<Currency> CurrencySet { get; private set; } 
	public IQueryable<Customer> CustomerSet { get; private set; } 
	public IQueryable<Date> DateSet { get; private set; } 
	public IQueryable<Employee> EmployeeSet { get; private set; } 
	public IQueryable<Geography> GeographySet { get; private set; } 
	public IQueryable<Product> ProductSet { get; private set; } 
	public IQueryable<ProductCategory> ProductCategorySet { get; private set; } 
	public IQueryable<ProductSubcategory> ProductSubcategorySet { get; private set; } 
	public IQueryable<Promotion> PromotionSet { get; private set; } 
	public IQueryable<Reseller> ResellerSet { get; private set; } 
	public IQueryable<SalesTerritory> SalesTerritorySet { get; private set; } 
	public IQueryable<InternetSales> InternetSalesSet { get; private set; } 
	public IQueryable<ProductInventory> ProductInventorySet { get; private set; } 
	public IQueryable<ResellerSales> ResellerSalesSet { get; private set; } 
	public IQueryable<SalesQuota> SalesQuotaSet { get; private set; } 
	public IQueryable<Numbers> NumbersSet { get; private set; } 
public AdventureWorksContext(string connectionString)
{
    var provider = new TabularQueryProvider(connectionString);
    provider.Log += System.Console.WriteLine;
    this.CurrencySet = new TabularTable<Currency>(provider);
    this.CustomerSet = new TabularTable<Customer>(provider);
    this.DateSet = new TabularTable<Date>(provider);
    this.EmployeeSet = new TabularTable<Employee>(provider);
    this.GeographySet = new TabularTable<Geography>(provider);
    this.ProductSet = new TabularTable<Product>(provider);
    this.ProductCategorySet = new TabularTable<ProductCategory>(provider);
    this.ProductSubcategorySet = new TabularTable<ProductSubcategory>(provider);
    this.PromotionSet = new TabularTable<Promotion>(provider);
    this.ResellerSet = new TabularTable<Reseller>(provider);
    this.SalesTerritorySet = new TabularTable<SalesTerritory>(provider);
    this.InternetSalesSet = new TabularTable<InternetSales>(provider);
    this.ProductInventorySet = new TabularTable<ProductInventory>(provider);
    this.ResellerSalesSet = new TabularTable<ResellerSales>(provider);
    this.SalesQuotaSet = new TabularTable<SalesQuota>(provider);
    this.NumbersSet = new TabularTable<Numbers>(provider);
}
}
[TabularTableMapping("'Currency'")]
public class Currency : ITabularData
{
[TabularMapping("'Currency'[CurrencyKey]", "'Currency'")]
public Int64 CurrencyKey {get; set;}
[TabularMapping("'Currency'[Currency Code]", "'Currency'")]
public String CurrencyCode {get; set;}
[TabularMapping("'Currency'[CurrencyName]", "'Currency'")]
public String CurrencyName {get; set;}
}
[TabularTableMapping("'Customer'")]
public class Customer : ITabularData
{
[TabularMapping("'Customer'[CustomerKey]", "'Customer'")]
public Int64 CustomerKey {get; set;}
[TabularMapping("'Customer'[GeographyKey]", "'Customer'")]
public Int64 GeographyKey {get; set;}
[TabularMapping("'Customer'[Customer Id]", "'Customer'")]
public String CustomerId {get; set;}
[TabularMapping("'Customer'[Title]", "'Customer'")]
public String Title {get; set;}
[TabularMapping("'Customer'[First Name]", "'Customer'")]
public String FirstName {get; set;}
[TabularMapping("'Customer'[Middle Name]", "'Customer'")]
public String MiddleName {get; set;}
[TabularMapping("'Customer'[Last Name]", "'Customer'")]
public String LastName {get; set;}
[TabularMapping("'Customer'[Name Style]", "'Customer'")]
public Boolean NameStyle {get; set;}
[TabularMapping("'Customer'[Birth Date]", "'Customer'")]
public DateTime BirthDate {get; set;}
[TabularMapping("'Customer'[Marital Status]", "'Customer'")]
public String MaritalStatus {get; set;}
[TabularMapping("'Customer'[Suffix]", "'Customer'")]
public String Suffix {get; set;}
[TabularMapping("'Customer'[Gender]", "'Customer'")]
public String Gender {get; set;}
[TabularMapping("'Customer'[Email Address]", "'Customer'")]
public String EmailAddress {get; set;}
[TabularMapping("'Customer'[Yearly Income]", "'Customer'")]
public Decimal YearlyIncome {get; set;}
[TabularMapping("'Customer'[Total Children]", "'Customer'")]
public Int64 TotalChildren {get; set;}
[TabularMapping("'Customer'[Number of Children At Home]", "'Customer'")]
public Int64 NumberOfChildrenAtHome {get; set;}
[TabularMapping("'Customer'[Education]", "'Customer'")]
public String Education {get; set;}
[TabularMapping("'Customer'[Occupation]", "'Customer'")]
public String Occupation {get; set;}
[TabularMapping("'Customer'[Owns House]", "'Customer'")]
public Boolean OwnsHouse {get; set;}
[TabularMapping("'Customer'[Total Cars Owned]", "'Customer'")]
public Int64 TotalCarsOwned {get; set;}
[TabularMapping("'Customer'[Address Line 1]", "'Customer'")]
public String AddressLine1 {get; set;}
[TabularMapping("'Customer'[Address Line 2]", "'Customer'")]
public String AddressLine2 {get; set;}
[TabularMapping("'Customer'[Phone]", "'Customer'")]
public String Phone {get; set;}
[TabularMapping("'Customer'[Date Of First Purchase]", "'Customer'")]
public DateTime DateOfFirstPurchase {get; set;}
[TabularMapping("'Customer'[Commute Distance]", "'Customer'")]
public String CommuteDistance {get; set;}
public virtual Geography RelatedGeography{ get; set;}
}
[TabularTableMapping("'Date'")]
public class Date : ITabularData
{
[TabularMapping("'Date'[DateKey]", "'Date'")]
public Int64 DateKey {get; set;}
[TabularMapping("'Date'[Date]", "'Date'")]
public DateTime Date2 {get; set;}
[TabularMapping("'Date'[Day Number Of Week]", "'Date'")]
public Int64 DayNumberOfWeek {get; set;}
[TabularMapping("'Date'[Day Name Of Week]", "'Date'")]
public String DayNameOfWeek {get; set;}
[TabularMapping("'Date'[Day Of Month]", "'Date'")]
public Int64 DayOfMonth {get; set;}
[TabularMapping("'Date'[Day Of Year]", "'Date'")]
public Int64 DayOfYear {get; set;}
[TabularMapping("'Date'[Week Of Year]", "'Date'")]
public Int64 WeekOfYear {get; set;}
[TabularMapping("'Date'[Month Name]", "'Date'")]
public String MonthName {get; set;}
[TabularMapping("'Date'[Month]", "'Date'")]
public Int64 Month {get; set;}
[TabularMapping("'Date'[Calendar Quarter]", "'Date'")]
public Int64 CalendarQuarter {get; set;}
[TabularMapping("'Date'[Calendar Year]", "'Date'")]
public Int64 CalendarYear {get; set;}
[TabularMapping("'Date'[Calendar Semester]", "'Date'")]
public Int64 CalendarSemester {get; set;}
[TabularMapping("'Date'[Fiscal Quarter]", "'Date'")]
public Int64 FiscalQuarter {get; set;}
[TabularMapping("'Date'[Fiscal Year]", "'Date'")]
public Int64 FiscalYear {get; set;}
[TabularMapping("'Date'[Fiscal Semester]", "'Date'")]
public Int64 FiscalSemester {get; set;}
[TabularMeasureMapping("[Days In Current Quarter]")]
public Int64 DaysInCurrentQuarter()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Days In Current Quarter]")]
public Int64 DaysInCurrentQuarter(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Days In Current Quarter to Date]")]
public Int64 DaysInCurrentQuarterToDate()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Days In Current Quarter to Date]")]
public Int64 DaysInCurrentQuarterToDate(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
}
[TabularTableMapping("'Employee'")]
public class Employee : ITabularData
{
[TabularMapping("'Employee'[EmployeeKey]", "'Employee'")]
public Int64 EmployeeKey {get; set;}
[TabularMapping("'Employee'[ParentEmployeeKey]", "'Employee'")]
public Int64 ParentEmployeeKey {get; set;}
[TabularMapping("'Employee'[Employee Id]", "'Employee'")]
public String EmployeeId {get; set;}
[TabularMapping("'Employee'[SalesTerritoryKey]", "'Employee'")]
public Int64 SalesTerritoryKey {get; set;}
[TabularMapping("'Employee'[First Name]", "'Employee'")]
public String FirstName {get; set;}
[TabularMapping("'Employee'[Last Name]", "'Employee'")]
public String LastName {get; set;}
[TabularMapping("'Employee'[Middle Name]", "'Employee'")]
public String MiddleName {get; set;}
[TabularMapping("'Employee'[NameStyle]", "'Employee'")]
public Boolean NameStyle {get; set;}
[TabularMapping("'Employee'[Title]", "'Employee'")]
public String Title {get; set;}
[TabularMapping("'Employee'[Hire Date]", "'Employee'")]
public DateTime HireDate {get; set;}
[TabularMapping("'Employee'[Birth Date]", "'Employee'")]
public DateTime BirthDate {get; set;}
[TabularMapping("'Employee'[Login]", "'Employee'")]
public String Login {get; set;}
[TabularMapping("'Employee'[Email]", "'Employee'")]
public String Email {get; set;}
[TabularMapping("'Employee'[Phone]", "'Employee'")]
public String Phone {get; set;}
[TabularMapping("'Employee'[Marital Status]", "'Employee'")]
public String MaritalStatus {get; set;}
[TabularMapping("'Employee'[Emergency Contact Name]", "'Employee'")]
public String EmergencyContactName {get; set;}
[TabularMapping("'Employee'[Emergency Contact Phone]", "'Employee'")]
public String EmergencyContactPhone {get; set;}
[TabularMapping("'Employee'[Is Salaried]", "'Employee'")]
public Boolean IsSalaried {get; set;}
[TabularMapping("'Employee'[Gender]", "'Employee'")]
public String Gender {get; set;}
[TabularMapping("'Employee'[Pay Frequency]", "'Employee'")]
public Int64 PayFrequency {get; set;}
[TabularMapping("'Employee'[Base Rate]", "'Employee'")]
public Decimal BaseRate {get; set;}
[TabularMapping("'Employee'[Vacation Hours]", "'Employee'")]
public Int64 VacationHours {get; set;}
[TabularMapping("'Employee'[Sick Leave Hours]", "'Employee'")]
public Int64 SickLeaveHours {get; set;}
[TabularMapping("'Employee'[Is Current]", "'Employee'")]
public Boolean IsCurrent {get; set;}
[TabularMapping("'Employee'[Is Sales Person]", "'Employee'")]
public Boolean IsSalesPerson {get; set;}
[TabularMapping("'Employee'[Department Name]", "'Employee'")]
public String DepartmentName {get; set;}
[TabularMapping("'Employee'[Start Date]", "'Employee'")]
public DateTime StartDate {get; set;}
[TabularMapping("'Employee'[End Date]", "'Employee'")]
public DateTime EndDate {get; set;}
[TabularMapping("'Employee'[Status]", "'Employee'")]
public String Status {get; set;}
public virtual SalesTerritory RelatedSalesTerritory{ get; set;}
}
[TabularTableMapping("'Geography'")]
public class Geography : ITabularData
{
[TabularMapping("'Geography'[GeographyKey]", "'Geography'")]
public Int64 GeographyKey {get; set;}
[TabularMapping("'Geography'[City]", "'Geography'")]
public String City {get; set;}
[TabularMapping("'Geography'[State Province Code]", "'Geography'")]
public String StateProvinceCode {get; set;}
[TabularMapping("'Geography'[State Province Name]", "'Geography'")]
public String StateProvinceName {get; set;}
[TabularMapping("'Geography'[Country Region Code]", "'Geography'")]
public String CountryRegionCode {get; set;}
[TabularMapping("'Geography'[Country Region Name]", "'Geography'")]
public String CountryRegionName {get; set;}
[TabularMapping("'Geography'[Postal Code]", "'Geography'")]
public String PostalCode {get; set;}
[TabularMapping("'Geography'[SalesTerritoryKey]", "'Geography'")]
public Int64 SalesTerritoryKey {get; set;}
public virtual SalesTerritory RelatedSalesTerritory{ get; set;}
}
[TabularTableMapping("'Product'")]
public class Product : ITabularData
{
[TabularMapping("'Product'[ProductKey]", "'Product'")]
public Int64 ProductKey {get; set;}
[TabularMapping("'Product'[Product Id]", "'Product'")]
public String ProductId {get; set;}
[TabularMapping("'Product'[ProductSubcategoryKey]", "'Product'")]
public Int64 ProductSubcategoryKey {get; set;}
[TabularMapping("'Product'[Weight Unit Code]", "'Product'")]
public String WeightUnitCode {get; set;}
[TabularMapping("'Product'[Size Unit Code]", "'Product'")]
public String SizeUnitCode {get; set;}
[TabularMapping("'Product'[Product Name]", "'Product'")]
public String ProductName {get; set;}
[TabularMapping("'Product'[Standard Cost]", "'Product'")]
public Decimal StandardCost {get; set;}
[TabularMapping("'Product'[Is Finished Goods]", "'Product'")]
public Boolean IsFinishedGoods {get; set;}
[TabularMapping("'Product'[Color]", "'Product'")]
public String Color {get; set;}
[TabularMapping("'Product'[Safety Stock Level]", "'Product'")]
public Int64 SafetyStockLevel {get; set;}
[TabularMapping("'Product'[Reorder Point]", "'Product'")]
public Int64 ReorderPoint {get; set;}
[TabularMapping("'Product'[List Price]", "'Product'")]
public Decimal ListPrice {get; set;}
[TabularMapping("'Product'[Size]", "'Product'")]
public String Size {get; set;}
[TabularMapping("'Product'[Size Range]", "'Product'")]
public String SizeRange {get; set;}
[TabularMapping("'Product'[Weight]", "'Product'")]
public Double Weight {get; set;}
[TabularMapping("'Product'[Days To Manufacture]", "'Product'")]
public Int64 DaysToManufacture {get; set;}
[TabularMapping("'Product'[Product Line]", "'Product'")]
public String ProductLine {get; set;}
[TabularMapping("'Product'[Dealer Price]", "'Product'")]
public Decimal DealerPrice {get; set;}
[TabularMapping("'Product'[Class]", "'Product'")]
public String Class {get; set;}
[TabularMapping("'Product'[Style]", "'Product'")]
public String Style {get; set;}
[TabularMapping("'Product'[Model Name]", "'Product'")]
public String ModelName {get; set;}
[TabularMapping("'Product'[Description]", "'Product'")]
public String Description {get; set;}
[TabularMapping("'Product'[Product Start Date]", "'Product'")]
public DateTime ProductStartDate {get; set;}
[TabularMapping("'Product'[Product End Date]", "'Product'")]
public DateTime ProductEndDate {get; set;}
[TabularMapping("'Product'[Product Status]", "'Product'")]
public String ProductStatus {get; set;}
[TabularMapping("'Product'[Product SubCategory Name]", "'Product'")]
public String ProductSubcategoryName {get; set;}
[TabularMapping("'Product'[Product Category Name]", "'Product'")]
public String ProductCategoryName {get; set;}
public virtual ProductSubcategory RelatedProductSubcategory{ get; set;}
}
[TabularTableMapping("'Product Category'")]
public class ProductCategory : ITabularData
{
[TabularMapping("'Product Category'[ProductCategoryKey]", "'Product Category'")]
public Int64 ProductCategoryKey {get; set;}
[TabularMapping("'Product Category'[Product Category Name]", "'Product Category'")]
public String ProductCategoryName {get; set;}
}
[TabularTableMapping("'Product Subcategory'")]
public class ProductSubcategory : ITabularData
{
[TabularMapping("'Product Subcategory'[ProductSubcategoryKey]", "'Product Subcategory'")]
public Int64 ProductSubcategoryKey {get; set;}
[TabularMapping("'Product Subcategory'[Product Subcategory Name]", "'Product Subcategory'")]
public String ProductSubcategoryName {get; set;}
[TabularMapping("'Product Subcategory'[ProductCategoryKey]", "'Product Subcategory'")]
public Int64 ProductCategoryKey {get; set;}
public virtual ProductCategory RelatedProductCategory{ get; set;}
}
[TabularTableMapping("'Promotion'")]
public class Promotion : ITabularData
{
[TabularMapping("'Promotion'[PromotionKey]", "'Promotion'")]
public Int64 PromotionKey {get; set;}
[TabularMapping("'Promotion'[Promotion Name]", "'Promotion'")]
public String PromotionName {get; set;}
[TabularMapping("'Promotion'[DiscountPct]", "'Promotion'")]
public Double DiscountPct {get; set;}
[TabularMapping("'Promotion'[Promotion Type]", "'Promotion'")]
public String PromotionType {get; set;}
[TabularMapping("'Promotion'[Promotion Category]", "'Promotion'")]
public String PromotionCategory {get; set;}
[TabularMapping("'Promotion'[Promotion Start Date]", "'Promotion'")]
public DateTime PromotionStartDate {get; set;}
[TabularMapping("'Promotion'[Promotion End Date]", "'Promotion'")]
public DateTime PromotionEndDate {get; set;}
[TabularMapping("'Promotion'[Min Quantity]", "'Promotion'")]
public Int64 MinQuantity {get; set;}
[TabularMapping("'Promotion'[Max Quantity]", "'Promotion'")]
public Int64 MaxQuantity {get; set;}
}
[TabularTableMapping("'Reseller'")]
public class Reseller : ITabularData
{
[TabularMapping("'Reseller'[ResellerKey]", "'Reseller'")]
public Int64 ResellerKey {get; set;}
[TabularMapping("'Reseller'[GeographyKey]", "'Reseller'")]
public Int64 GeographyKey {get; set;}
[TabularMapping("'Reseller'[Reseller Id]", "'Reseller'")]
public String ResellerId {get; set;}
[TabularMapping("'Reseller'[Reseller Phone]", "'Reseller'")]
public String ResellerPhone {get; set;}
[TabularMapping("'Reseller'[Business Type]", "'Reseller'")]
public String BusinessType {get; set;}
[TabularMapping("'Reseller'[Reseller Name]", "'Reseller'")]
public String ResellerName {get; set;}
[TabularMapping("'Reseller'[Number Employees]", "'Reseller'")]
public Int64 NumberEmployees {get; set;}
[TabularMapping("'Reseller'[Order Frequency]", "'Reseller'")]
public String OrderFrequency {get; set;}
[TabularMapping("'Reseller'[Order Month]", "'Reseller'")]
public Int64 OrderMonth {get; set;}
[TabularMapping("'Reseller'[First Order Year]", "'Reseller'")]
public Int64 FirstOrderYear {get; set;}
[TabularMapping("'Reseller'[Last Order Year]", "'Reseller'")]
public Int64 LastOrderYear {get; set;}
[TabularMapping("'Reseller'[Product Line]", "'Reseller'")]
public String ProductLine {get; set;}
[TabularMapping("'Reseller'[Address Line 1]", "'Reseller'")]
public String AddressLine1 {get; set;}
[TabularMapping("'Reseller'[Address Line 2]", "'Reseller'")]
public String AddressLine2 {get; set;}
[TabularMapping("'Reseller'[Annual Sales]", "'Reseller'")]
public Decimal AnnualSales {get; set;}
[TabularMapping("'Reseller'[Bank Name]", "'Reseller'")]
public String BankName {get; set;}
[TabularMapping("'Reseller'[Min Payment Type]", "'Reseller'")]
public Int64 MinPaymentType {get; set;}
[TabularMapping("'Reseller'[Min Payment Amount]", "'Reseller'")]
public Decimal MinPaymentAmount {get; set;}
[TabularMapping("'Reseller'[Annua Revenue]", "'Reseller'")]
public Decimal AnnuaRevenue {get; set;}
[TabularMapping("'Reseller'[Year Opened]", "'Reseller'")]
public Int64 YearOpened {get; set;}
public virtual Geography RelatedGeography{ get; set;}
}
[TabularTableMapping("'Sales Territory'")]
public class SalesTerritory : ITabularData
{
[TabularMapping("'Sales Territory'[SalesTerritoryKey]", "'Sales Territory'")]
public Int64 SalesTerritoryKey {get; set;}
[TabularMapping("'Sales Territory'[Sales Territory Region]", "'Sales Territory'")]
public String SalesTerritoryRegion {get; set;}
[TabularMapping("'Sales Territory'[Sales Territory Country]", "'Sales Territory'")]
public String SalesTerritoryCountry {get; set;}
[TabularMapping("'Sales Territory'[Sales Territory Group]", "'Sales Territory'")]
public String SalesTerritoryGroup {get; set;}
[TabularMeasureMapping("[Total Previous Quarter Gross Profit]")]
public Decimal TotalPreviousQuarterGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Previous Quarter Gross Profit]")]
public Decimal TotalPreviousQuarterGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Gross Profit Performance]")]
public Double TotalCurrentQuarterGrossProfitPerformance()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Gross Profit Performance]")]
public Double TotalCurrentQuarterGrossProfitPerformance(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Distinct Count Sales Orders]")]
public Int64 DistinctCountSalesOrders()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Distinct Count Sales Orders]")]
public Int64 DistinctCountSalesOrders(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Gross Profit]")]
public Decimal TotalGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Gross Profit]")]
public Decimal TotalGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Sales - Sales Territory sliced by Employee]")]
public Decimal TotalSalesSalesTerritorySlicedByEmployee()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Sales - Sales Territory sliced by Employee]")]
public Decimal TotalSalesSalesTerritorySlicedByEmployee(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Sales]")]
public Decimal TotalSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Sales]")]
public Decimal TotalSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Sales]")]
public Decimal TotalCurrentQuarterSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Sales]")]
public Decimal TotalCurrentQuarterSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Sales Performance]")]
public Double TotalCurrentQuarterSalesPerformance()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Sales Performance]")]
public Double TotalCurrentQuarterSalesPerformance(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Previous Quarter Sales]")]
public Decimal TotalPreviousQuarterSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Previous Quarter Sales]")]
public Decimal TotalPreviousQuarterSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Previous Quarter Gross Profit Proportion to QTD]")]
public Decimal TotalPreviousQuarterGrossProfitProportionToQtd()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Previous Quarter Gross Profit Proportion to QTD]")]
public Decimal TotalPreviousQuarterGrossProfitProportionToQtd(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Products Cost]")]
public Decimal TotalProductsCost()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Products Cost]")]
public Decimal TotalProductsCost(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Gross Profit]")]
public Decimal TotalCurrentQuarterGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Current Quarter Gross Profit]")]
public Decimal TotalCurrentQuarterGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units Sold]")]
public Int64 TotalUnitsSold()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units Sold]")]
public Int64 TotalUnitsSold(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Freight]")]
public Decimal TotalFreight()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Freight]")]
public Decimal TotalFreight(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Previous Quarter Sales Proportion to QTD]")]
public Decimal TotalPreviousQuarterSalesProportionToQtd()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Previous Quarter Sales Proportion to QTD]")]
public Decimal TotalPreviousQuarterSalesProportionToQtd(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Tax Amount]")]
public Decimal TotalTaxAmount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Tax Amount]")]
public Decimal TotalTaxAmount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Discount Amount]")]
public Decimal TotalDiscountAmount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Discount Amount]")]
public Decimal TotalDiscountAmount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Order Lines Count]")]
public Int64 OrderLinesCount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Order Lines Count]")]
public Int64 OrderLinesCount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
}
[TabularTableMapping("'Internet Sales'")]
public class InternetSales : ITabularData
{
[TabularMapping("'Internet Sales'[ProductKey]", "'Internet Sales'")]
public Int64 ProductKey {get; set;}
[TabularMapping("'Internet Sales'[OrderDateKey]", "'Internet Sales'")]
public Int64 OrderDateKey {get; set;}
[TabularMapping("'Internet Sales'[DueDateKey]", "'Internet Sales'")]
public Int64 DueDateKey {get; set;}
[TabularMapping("'Internet Sales'[ShipDateKey]", "'Internet Sales'")]
public Int64 ShipDateKey {get; set;}
[TabularMapping("'Internet Sales'[CustomerKey]", "'Internet Sales'")]
public Int64 CustomerKey {get; set;}
[TabularMapping("'Internet Sales'[PromotionKey]", "'Internet Sales'")]
public Int64 PromotionKey {get; set;}
[TabularMapping("'Internet Sales'[CurrencyKey]", "'Internet Sales'")]
public Int64 CurrencyKey {get; set;}
[TabularMapping("'Internet Sales'[SalesTerritoryKey]", "'Internet Sales'")]
public Int64 SalesTerritoryKey {get; set;}
[TabularMapping("'Internet Sales'[Sales Order Number]", "'Internet Sales'")]
public String SalesOrderNumber {get; set;}
[TabularMapping("'Internet Sales'[Sales Order Line Number]", "'Internet Sales'")]
public Int64 SalesOrderLineNumber {get; set;}
[TabularMapping("'Internet Sales'[Revision Number]", "'Internet Sales'")]
public Int64 RevisionNumber {get; set;}
[TabularMapping("'Internet Sales'[Order Quantity]", "'Internet Sales'")]
public Int64 OrderQuantity {get; set;}
[TabularMapping("'Internet Sales'[Unit Price]", "'Internet Sales'")]
public Decimal UnitPrice {get; set;}
[TabularMapping("'Internet Sales'[Extended Amount]", "'Internet Sales'")]
public Decimal ExtendedAmount {get; set;}
[TabularMapping("'Internet Sales'[Unit Price Discount Pct]", "'Internet Sales'")]
public Double UnitPriceDiscountPct {get; set;}
[TabularMapping("'Internet Sales'[Discount Amount]", "'Internet Sales'")]
public Decimal DiscountAmount {get; set;}
[TabularMapping("'Internet Sales'[Product Standard Cost]", "'Internet Sales'")]
public Decimal ProductStandardCost {get; set;}
[TabularMapping("'Internet Sales'[Total Product Cost]", "'Internet Sales'")]
public Decimal TotalProductCost {get; set;}
[TabularMapping("'Internet Sales'[Sales Amount]", "'Internet Sales'")]
public Decimal SalesAmount {get; set;}
[TabularMapping("'Internet Sales'[Tax Amount]", "'Internet Sales'")]
public Decimal TaxAmount {get; set;}
[TabularMapping("'Internet Sales'[Freight]", "'Internet Sales'")]
public Decimal Freight {get; set;}
[TabularMapping("'Internet Sales'[Carrier Tracking Number]", "'Internet Sales'")]
public String CarrierTrackingNumber {get; set;}
[TabularMapping("'Internet Sales'[Customer PO Number]", "'Internet Sales'")]
public String CustomerPoNumber {get; set;}
[TabularMapping("'Internet Sales'[Order Date]", "'Internet Sales'")]
public DateTime OrderDate {get; set;}
[TabularMapping("'Internet Sales'[Due Date]", "'Internet Sales'")]
public DateTime DueDate {get; set;}
[TabularMapping("'Internet Sales'[Ship Date]", "'Internet Sales'")]
public DateTime ShipDate {get; set;}
[TabularMapping("'Internet Sales'[Gross Profit]", "'Internet Sales'")]
public Decimal GrossProfit {get; set;}
[TabularMeasureMapping("[Internet Current Quarter Gross Profit Performance]")]
public Double InternetCurrentQuarterGrossProfitPerformance()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Current Quarter Gross Profit Performance]")]
public Double InternetCurrentQuarterGrossProfitPerformance(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Units]")]
public Int64 InternetTotalUnits()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Units]")]
public Int64 InternetTotalUnits(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Sales]")]
public Decimal InternetPreviousQuarterSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Sales]")]
public Decimal InternetPreviousQuarterSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Current Quarter Gross Profit]")]
public Decimal InternetCurrentQuarterGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Current Quarter Gross Profit]")]
public Decimal InternetCurrentQuarterGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Order Lines Count]")]
public Int64 InternetOrderLinesCount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Order Lines Count]")]
public Int64 InternetOrderLinesCount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Discount Amount]")]
public Decimal InternetTotalDiscountAmount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Discount Amount]")]
public Decimal InternetTotalDiscountAmount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Sales]")]
public Decimal InternetTotalSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Sales]")]
public Decimal InternetTotalSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Gross Profit]")]
public Decimal InternetTotalGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Gross Profit]")]
public Decimal InternetTotalGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Freight]")]
public Decimal InternetTotalFreight()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Freight]")]
public Decimal InternetTotalFreight(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Product Cost]")]
public Decimal InternetTotalProductCost()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Product Cost]")]
public Decimal InternetTotalProductCost(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Current Quarter Sales Performance]")]
public Double InternetCurrentQuarterSalesPerformance()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Current Quarter Sales Performance]")]
public Double InternetCurrentQuarterSalesPerformance(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Gross Profit]")]
public Decimal InternetPreviousQuarterGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Gross Profit]")]
public Decimal InternetPreviousQuarterGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Sales Proportion to QTD]")]
public Decimal InternetPreviousQuarterSalesProportionToQtd()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Sales Proportion to QTD]")]
public Decimal InternetPreviousQuarterSalesProportionToQtd(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Current Quarter Sales]")]
public Decimal InternetCurrentQuarterSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Current Quarter Sales]")]
public Decimal InternetCurrentQuarterSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Tax Amount]")]
public Decimal InternetTotalTaxAmount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Total Tax Amount]")]
public Decimal InternetTotalTaxAmount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Gross Profit Proportion to QTD]")]
public Decimal InternetPreviousQuarterGrossProfitProportionToQtd()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Previous Quarter Gross Profit Proportion to QTD]")]
public Decimal InternetPreviousQuarterGrossProfitProportionToQtd(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Distinct Count Sales Order]")]
public Int64 InternetDistinctCountSalesOrder()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Internet Distinct Count Sales Order]")]
public Int64 InternetDistinctCountSalesOrder(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
public virtual Currency RelatedCurrency{ get; set;}
public virtual Customer RelatedCustomer{ get; set;}
public virtual Date RelatedDate{ get; set;}
public virtual Product RelatedProduct{ get; set;}
public virtual Promotion RelatedPromotion{ get; set;}
public virtual SalesTerritory RelatedSalesTerritory{ get; set;}
}
[TabularTableMapping("'Product Inventory'")]
public class ProductInventory : ITabularData
{
[TabularMapping("'Product Inventory'[ProductKey]", "'Product Inventory'")]
public Int64 ProductKey {get; set;}
[TabularMapping("'Product Inventory'[DateKey]", "'Product Inventory'")]
public Int64 DateKey {get; set;}
[TabularMapping("'Product Inventory'[Movement Date]", "'Product Inventory'")]
public DateTime MovementDate {get; set;}
[TabularMapping("'Product Inventory'[Unit Cost]", "'Product Inventory'")]
public Decimal UnitCost {get; set;}
[TabularMapping("'Product Inventory'[Units In]", "'Product Inventory'")]
public Int64 UnitsIn {get; set;}
[TabularMapping("'Product Inventory'[Units Out]", "'Product Inventory'")]
public Int64 UnitsOut {get; set;}
[TabularMapping("'Product Inventory'[Units Balance]", "'Product Inventory'")]
public Int64 UnitsBalance {get; set;}
[TabularMapping("'Product Inventory'[Product-Date Inventory Value]", "'Product Inventory'")]
public Decimal ProductDateInventoryValue {get; set;}
[TabularMapping("'Product Inventory'[Product-Date Optimal Inventory Value]", "'Product Inventory'")]
public Decimal ProductDateOptimalInventoryValue {get; set;}
[TabularMapping("'Product Inventory'[Product-Date Max Inventory Value]", "'Product Inventory'")]
public Decimal ProductDateMaxInventoryValue {get; set;}
[TabularMapping("'Product Inventory'[Product-Date OverStocked]", "'Product Inventory'")]
public Int64 ProductDateOverstocked {get; set;}
[TabularMapping("'Product Inventory'[Product-Date UnderStocked]", "'Product Inventory'")]
public Int64 ProductDateUnderstocked {get; set;}
[TabularMapping("'Product Inventory'[Product-Date Negative Stock]", "'Product Inventory'")]
public Int64 ProductDateNegativeStock {get; set;}
[TabularMeasureMapping("[Total Units Movement]")]
public Int64 TotalUnitsMovement()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units Movement]")]
public Int64 TotalUnitsMovement(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Value Performance]")]
public Double TotalInventoryValuePerformance()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Value Performance]")]
public Double TotalInventoryValuePerformance(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Products with Negative Stock]")]
public Int64 ProductsWithNegativeStock()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Products with Negative Stock]")]
public Int64 ProductsWithNegativeStock(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Optimal Value]")]
public Decimal TotalInventoryOptimalValue()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Optimal Value]")]
public Decimal TotalInventoryOptimalValue(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units In]")]
public Int64 TotalUnitsIn()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units In]")]
public Int64 TotalUnitsIn(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Value]")]
public Decimal TotalInventoryValue()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Value]")]
public Decimal TotalInventoryValue(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units]")]
public Int64 TotalUnits()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units]")]
public Int64 TotalUnits(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Maximum Value]")]
public Decimal TotalInventoryMaximumValue()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Inventory Maximum Value]")]
public Decimal TotalInventoryMaximumValue(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units Out]")]
public Int64 TotalUnitsOut()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Total Units Out]")]
public Int64 TotalUnitsOut(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Products UnderStocked]")]
public Int64 ProductsUnderstocked()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Products UnderStocked]")]
public Int64 ProductsUnderstocked(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Products OverStocked]")]
public Int64 ProductsOverstocked()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Products OverStocked]")]
public Int64 ProductsOverstocked(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
public virtual Date RelatedDate{ get; set;}
public virtual Product RelatedProduct{ get; set;}
}
[TabularTableMapping("'Reseller Sales'")]
public class ResellerSales : ITabularData
{
[TabularMapping("'Reseller Sales'[ProductKey]", "'Reseller Sales'")]
public Int64 ProductKey {get; set;}
[TabularMapping("'Reseller Sales'[OrderDateKey]", "'Reseller Sales'")]
public Int64 OrderDateKey {get; set;}
[TabularMapping("'Reseller Sales'[DueDateKey]", "'Reseller Sales'")]
public Int64 DueDateKey {get; set;}
[TabularMapping("'Reseller Sales'[ShipDateKey]", "'Reseller Sales'")]
public Int64 ShipDateKey {get; set;}
[TabularMapping("'Reseller Sales'[ResellerKey]", "'Reseller Sales'")]
public Int64 ResellerKey {get; set;}
[TabularMapping("'Reseller Sales'[EmployeeKey]", "'Reseller Sales'")]
public Int64 EmployeeKey {get; set;}
[TabularMapping("'Reseller Sales'[PromotionKey]", "'Reseller Sales'")]
public Int64 PromotionKey {get; set;}
[TabularMapping("'Reseller Sales'[CurrencyKey]", "'Reseller Sales'")]
public Int64 CurrencyKey {get; set;}
[TabularMapping("'Reseller Sales'[SalesTerritoryKey]", "'Reseller Sales'")]
public Int64 SalesTerritoryKey {get; set;}
[TabularMapping("'Reseller Sales'[Sales Order Number]", "'Reseller Sales'")]
public String SalesOrderNumber {get; set;}
[TabularMapping("'Reseller Sales'[Sales Order Line Number]", "'Reseller Sales'")]
public Int64 SalesOrderLineNumber {get; set;}
[TabularMapping("'Reseller Sales'[Revision Number]", "'Reseller Sales'")]
public Int64 RevisionNumber {get; set;}
[TabularMapping("'Reseller Sales'[Order Quantity]", "'Reseller Sales'")]
public Int64 OrderQuantity {get; set;}
[TabularMapping("'Reseller Sales'[Unit Price]", "'Reseller Sales'")]
public Decimal UnitPrice {get; set;}
[TabularMapping("'Reseller Sales'[Extended Amount]", "'Reseller Sales'")]
public Decimal ExtendedAmount {get; set;}
[TabularMapping("'Reseller Sales'[Unit Price Discount Pct]", "'Reseller Sales'")]
public Double UnitPriceDiscountPct {get; set;}
[TabularMapping("'Reseller Sales'[Discount Amount]", "'Reseller Sales'")]
public Decimal DiscountAmount {get; set;}
[TabularMapping("'Reseller Sales'[Product Standard Cost]", "'Reseller Sales'")]
public Decimal ProductStandardCost {get; set;}
[TabularMapping("'Reseller Sales'[Total Product Cost]", "'Reseller Sales'")]
public Decimal TotalProductCost {get; set;}
[TabularMapping("'Reseller Sales'[Sales Amount]", "'Reseller Sales'")]
public Decimal SalesAmount {get; set;}
[TabularMapping("'Reseller Sales'[Tax Amount]", "'Reseller Sales'")]
public Decimal TaxAmount {get; set;}
[TabularMapping("'Reseller Sales'[Freight]", "'Reseller Sales'")]
public Decimal Freight {get; set;}
[TabularMapping("'Reseller Sales'[Carrier Tracking Number]", "'Reseller Sales'")]
public String CarrierTrackingNumber {get; set;}
[TabularMapping("'Reseller Sales'[Reseller PO Number]", "'Reseller Sales'")]
public String ResellerPoNumber {get; set;}
[TabularMapping("'Reseller Sales'[Order Date]", "'Reseller Sales'")]
public DateTime OrderDate {get; set;}
[TabularMapping("'Reseller Sales'[Due Date]", "'Reseller Sales'")]
public DateTime DueDate {get; set;}
[TabularMapping("'Reseller Sales'[Ship Date]", "'Reseller Sales'")]
public DateTime ShipDate {get; set;}
[TabularMapping("'Reseller Sales'[Gross Profit]", "'Reseller Sales'")]
public Decimal GrossProfit {get; set;}
[TabularMeasureMapping("[Reseller Total Freight]")]
public Decimal ResellerTotalFreight()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Freight]")]
public Decimal ResellerTotalFreight(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Sales]")]
public Decimal ResellerPreviousQuarterSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Sales]")]
public Decimal ResellerPreviousQuarterSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Gross Profit Performance]")]
public Double ResellerCurrentQuarterGrossProfitPerformance()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Gross Profit Performance]")]
public Double ResellerCurrentQuarterGrossProfitPerformance(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Distinct Count Sales Order]")]
public Int64 ResellerDistinctCountSalesOrder()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Distinct Count Sales Order]")]
public Int64 ResellerDistinctCountSalesOrder(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Sales Performance]")]
public Double ResellerCurrentQuarterSalesPerformance()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Sales Performance]")]
public Double ResellerCurrentQuarterSalesPerformance(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Gross Profit Proportion to QTD]")]
public Decimal ResellerPreviousQuarterGrossProfitProportionToQtd()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Gross Profit Proportion to QTD]")]
public Decimal ResellerPreviousQuarterGrossProfitProportionToQtd(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Sales - Sales Territory sliced by Employee]")]
public Decimal ResellerTotalSalesSalesTerritorySlicedByEmployee()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Sales - Sales Territory sliced by Employee]")]
public Decimal ResellerTotalSalesSalesTerritorySlicedByEmployee(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Discount Amount]")]
public Decimal ResellerTotalDiscountAmount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Discount Amount]")]
public Decimal ResellerTotalDiscountAmount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Sales - Sales Territory sliced by Reseller]")]
public Decimal ResellerTotalSalesSalesTerritorySlicedByReseller()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Sales - Sales Territory sliced by Reseller]")]
public Decimal ResellerTotalSalesSalesTerritorySlicedByReseller(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Units]")]
public Int64 ResellerTotalUnits()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Units]")]
public Int64 ResellerTotalUnits(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Product Cost]")]
public Decimal ResellerTotalProductCost()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Product Cost]")]
public Decimal ResellerTotalProductCost(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Sales]")]
public Decimal ResellerCurrentQuarterSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Sales]")]
public Decimal ResellerCurrentQuarterSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Gross Profit]")]
public Decimal ResellerTotalGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Gross Profit]")]
public Decimal ResellerTotalGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Order Lines Count]")]
public Int64 ResellerOrderLinesCount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Order Lines Count]")]
public Int64 ResellerOrderLinesCount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Gross Profit]")]
public Decimal ResellerCurrentQuarterGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Current Quarter Gross Profit]")]
public Decimal ResellerCurrentQuarterGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Gross Profit]")]
public Decimal ResellerPreviousQuarterGrossProfit()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Gross Profit]")]
public Decimal ResellerPreviousQuarterGrossProfit(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Sales Proportion to QTD]")]
public Decimal ResellerPreviousQuarterSalesProportionToQtd()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Previous Quarter Sales Proportion to QTD]")]
public Decimal ResellerPreviousQuarterSalesProportionToQtd(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Sales]")]
public Decimal ResellerTotalSales()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Sales]")]
public Decimal ResellerTotalSales(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Tax Amount]")]
public Decimal ResellerTotalTaxAmount()
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
[TabularMeasureMapping("[Reseller Total Tax Amount]")]
public Decimal ResellerTotalTaxAmount(bool filter)
{
		throw new NotImplementedException("This method is only available in a LinqToDAX Query");
}
public virtual Currency RelatedCurrency{ get; set;}
public virtual Date RelatedDate{ get; set;}
public virtual Employee RelatedEmployee{ get; set;}
public virtual Product RelatedProduct{ get; set;}
public virtual Promotion RelatedPromotion{ get; set;}
public virtual Reseller RelatedReseller{ get; set;}
public virtual SalesTerritory RelatedSalesTerritory{ get; set;}
}
[TabularTableMapping("'Sales Quota'")]
public class SalesQuota : ITabularData
{
[TabularMapping("'Sales Quota'[SalesQuotaKey]", "'Sales Quota'")]
public Int64 SalesQuotaKey {get; set;}
[TabularMapping("'Sales Quota'[EmployeeKey]", "'Sales Quota'")]
public Int64 EmployeeKey {get; set;}
[TabularMapping("'Sales Quota'[DateKey]", "'Sales Quota'")]
public Int64 DateKey {get; set;}
[TabularMapping("'Sales Quota'[Calendar Year]", "'Sales Quota'")]
public Int64 CalendarYear {get; set;}
[TabularMapping("'Sales Quota'[Calendar Quarter]", "'Sales Quota'")]
public Int64 CalendarQuarter {get; set;}
[TabularMapping("'Sales Quota'[Sales Amount Quota]", "'Sales Quota'")]
public Decimal SalesAmountQuota {get; set;}
[TabularMapping("'Sales Quota'[Date]", "'Sales Quota'")]
public DateTime Date {get; set;}
public virtual Date RelatedDate{ get; set;}
public virtual Employee RelatedEmployee{ get; set;}
}
[TabularTableMapping("'Numbers'")]
public class Numbers : ITabularData
{
[TabularMapping("'Numbers'[ID]", "'Numbers'")]
public Int64 ID {get; set;}
}
}
