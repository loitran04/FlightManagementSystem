#  Flight Management System - Installation Guide

## 1. System Requirements

- **Operating System:** Windows 10 or later  
- **.NET Framework:** 4.7.2 or higher  
- **Database:** SQL Server Express or full version  
- **IDE:** Visual Studio (latest version recommended)

---

## 2. Installation and Running the Project

###  Step 1: Clone the Repository

```bash
git clone https://github.com/haothach/FlightManagementSystem.git
```

### 🗄 Step 2: Configure the Database

Import the SQL file into SQL Server:

```text
1. Open SQL Server Management Studio (SSMS)  
2. Connect to your SQL Server  
3. Execute the QLChuyenBay.sql file to create the database
```

### 🔧 Step 3: Configure the Connection String

Edit the connection string in your C# code:

```csharp
string cnStr = "Data Source=.;Initial Catalog=FlightManagement;Integrated Security=True";
```

###  Step 4: Run the Application

```text
1. Open Visual Studio  
2. Open the FlightManagement.sln project  
3. Press F5 to start the application
```

---

## 3.  Extended Features

-  **Email Sending:** Automatically sends ticket confirmation and flight update emails  
-  **Statistics & Charts:** Visualizes ticket sales and revenue trends  
-  **Report Exporting:** Exports reports to PDF or Excel files  
-  **Basic Chatbot:** Provides quick user support via a built-in chatbot

---

## 4.  Contact

If you have any questions or need support, feel free to contact:

**Email:** [haonhut.thach@gmail.com](mailto:haonhut.thach@gmail.com)
