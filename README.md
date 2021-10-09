# Intro to Quantum Computing

## Power Point Deck
You can download the presentation deck at: https://oneangrypenguin-my.sharepoint.com/:p:/p/samara/EfN9i6ZPmcZPlk5xQ1SP2y0Bu_-Fv_EKGbBxlBsBw679zA?e=Ary2Z0

## Q# Samples
The Q# sample code used in this repo can be found at github.com/Microsoft/Quantum. Specifically the Azure Quantum Parallel QRNG sample: https://github.com/microsoft/Quantum/tree/main/samples/azure-quantum/parallel-qrng

## To run the project
1. Create the Azure resources
	1. Quantum Workspace with IonQ provider **Note: Since IonQ is a third party product, it is not possible to use this service using some subscription types including many credit types.**
	2. Azure Functions with .NET runtime. Consumption plan is fine. This doesn't need to be in the same subscription/resource group as the quantum workspace.
2. Configure the Function App
	1. Under Identity, turn on System Assigned Managed Identity.
	2. Add the new identity as a Contributor on the quantum workspace and quantum workspace's storage account.
	3. Add the relevant App Settings (you can use local.settings.json.template):
		- subId: The subscription ID for the quantum workspace
		- quantumStorage: Link for the quantum workspace storage. https://<storage account>.blob.core.windows.net
		- quantumStorageConn: Under Access keys in the quantum workspace storage, grab the connection string
		- workspace: Name of the quantum workspace
		- resourceGroup: Quantum workspace resource group
		- location: Quantum workspace location. You can get the name from `az account list-locations -o table` in the CLI, or most of them are in [this Stack answer](https://stackoverflow.com/questions/44143981/is-there-an-api-to-list-all-azure-regions)
		- target: Use ionq.simulator or ionq.qpu depending on whether you want to target the simulator or the quantum hardware. It is likely the simulator will complete jobs much quicker.
3. Configure the Function App's storage account
	1. Add two tables 'jobs' and 'numbers'
	2. Create two containers 'rawfiles' and 'site'
	3. Into the 'rawfiles' folder, upload the three files in the jobfiles folder and the page/template.html file.
	4. Add page/template.js to the site container. 
4. Deploy the function app code.

Before the site will work all three functions will need to run. If you want to do this more quickly, you can target the ionq.simulator and trigger each function in turn from the portal or the management API.
Alternatively, you can add a row to the numbers table { 'PartitionKey': 1, 'RowKey': temp, 'Numbers': '[0,0,0,0]', 'State': 'new'} and run just the UpdatePage function to generate the template.

Keep in mind that as long as the site is running you will continue to incur costs from the quantum jobs even if the function app is within the consumption plan free grant or on credits. To prevent this without deleting the resources, stop the app or disable the CreateJob function.