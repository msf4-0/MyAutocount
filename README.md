# MyAutocount

This program contains C# functions that used to perform Autocount CRUD operations, REST API server and Windows service.

1. Clone the repository from Github.
- `git clone https://github.com/msf4-0/MyAutocount`
2. Run Powershell with Administrator by right click Start button or pressing Win + X.
3. Navigate to the folder containing MyAutocount.exe.
- `cd MyFolder\MyAutocount\MyAutocount\bin\Release`
4. Install the program as a service using the following command.
- `.\MyAutocount.exe install --delayed start`
5. Check the service in Windows Services.
- `Win + R > services.msc`
6. A service called MyAutocount API Server will be found. The startup type should be “Automatic (Delayed Start)” and service status should be “Running”.

 
