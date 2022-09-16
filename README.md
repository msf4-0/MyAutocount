# MyAutocount

This program contains C# functions that used to perform Autocount CRUD operations, REST API server and Windows service.

1. Clone the repository from Github.
- `git clone https://github.com/msf4-0/MyAutocount`

2. Run Powershell with Administrator by right click Start button or pressing Win + X.

![image](https://user-images.githubusercontent.com/69132663/190688545-cba4e2f6-5ab1-4603-8021-834bd06b00ed.png)

3. Navigate to the folder containing MyAutocount.exe.
- `cd MyFolder\MyAutocount\MyAutocount\bin\Release`

4. Install the program as a service using the following command.
- `.\MyAutocount.exe install --delayed start`

5. Check the service in Windows Services.
- `Win + R > services.msc`

6. A service called MyAutocount API Server will be found. The startup type should be “Automatic (Delayed Start)” and service status should be “Running”.

![image](https://user-images.githubusercontent.com/69132663/190689101-9e737149-305e-4eac-bcf2-7faa791a786a.png)

 
