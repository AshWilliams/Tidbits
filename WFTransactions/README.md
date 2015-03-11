# Workflow Services and Transactions

## What's this about?

This solution provides sample code that illustrates a possible issue with starting workflow services within a distributed transaction when the following conditions apply:

* The workflow instance store is the default SQL Server instance store;
* The client application opens an SQL Connection to the same SQL Server instance used as workflow instance store.

The issue in question is a race condition so it is likely that it will not be triggered most of the times.

## How can I run this?

You need to have Microsoft AppFabric installed and also update the following configuration files:

* Hosting\Web.config
	* Update the **WFInstanceStore** connection string to point to the AppFabric instance store database;
	* Update the workflow instance management service behavior to point to the administrators Windows groups configured in AppFabric (if you did a default AppFabric install, there should be no need to update this).
* Client\App.config
	* Update the application setting **WorkflowServiceAddress** to point to the URL where you are locally running the Hosting application;
	* Update the connection string **Master** to point to the SQL Server instance used as the workflow instance store (you can leave it pointing to the master database since the client application only performs read operations against it).

After ensuring the following prerequisites its a matter of running the client application a sufficient amount of times to trigger the race condition.

## I would like to know more?

See [Workflow services and distributed transactions](https://exceptionalcode.wordpress.com/2015/03/11/workflow-services-and-distributed-transactions-argh/).