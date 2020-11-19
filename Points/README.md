# Points
This is in response to the exercise prompt located here: https://fetch-hiring.s3.us-east-1.amazonaws.com/points.pdf

I chose to implement my API using .NET Core 5.0 because it gave me a good opportunity to try out this new version which was just released a few weeks ago, but also because I have more familiarity with C#. You could implement something similar in Ruby, PHP, Java, etc. I also don't know what system the person who is evaluating this would be using and .NET Core is more platform agnostic and doesn't require a specific SDK installed in order to run the built package, unlike .NET Framework projects. The build is OS dependent though (see Run process). 

The core logic driving the API is located in the Points.Api folder. All the endpoints are located within the Points controller (/points/ route), and they follow a generally RESTful approach, so there's GET, POST, DELETE endpoints. The API uses the Swashbuckle library to generate Swagger documentation, so if you're just interested in running it rather than building it, that's available at /swagger/ route.

## Test suite
I provided some tests for the application logic in the Points.Test project. Edge cases like adding negative points prompted me to decide that tests were going to be helpful. I wrote some basic tests around adding points and then used the example in the prompt as a major test case. All the test cases passed when I ran it locally.

## Build process
If you plan to build the process, you'll likely need Visual Studio 2019 and the .NET 5.0 SDK.

## Run process
If you merely want to run the process, I've provided a build for win-x64 architectures [here](https://drive.google.com/file/d/1IiXy1Y0lRNqUeA2x2KbdrYBejOaMgmXL/view?usp=sharing), but I can provide a build for other architectures if necessary. Here's some documentation on [possible architectures](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) (generally, Windows, Linx, and Mac are possible).

## Key points of implementation
A lot of the source code is specific to running a general .NET Core web application and can be ignored generally. There is some dependency injection happening through, so be aware of that. Key files to browse:
 - Points.Api/Application/* (Nearly all the application logic)
 - Points.Api/Controllers/PointsController.cs (The controller that handles the REST endpoints)
 - Points.Api/Models/* (The two models)
 - Points.Tests/PointsServiceTests.cs (The tests I wrote)
 
## 'add-sql' revision
In this second iteration of the project, I addressed two major areas of improvement:
 - ** Changed the data layer to an in-memory instance of SQLite.** This change was made to both the tests and the core project. In my previous "Next steps", I spoke to the drawbacks of using the memory cache as a data source and the issue with the data structure. Using SQLite let me make all the improvements to the data structure, while still keeping the scope of the project small. At this point, you could very easily swap the SQLite dependency for an actual relationship DB with little effort (create a new instance of IDbContext and maybe tweak PointsTransactionSqlRepository). However, I would keep the SQLite depedency for the tests as it works quite nicely running those.
 - ** Refactored the delete/deduction logic in PointsService.cs.** There was some code repetition that I wasn't happy about. In general, you should try to reduce repetition unless it reduces clarity significantly. In this case, I was able to add a private helper method that both the DeductPoints and AddPoints (with negative input) methods called.

## Next steps
Areas I would expand upon with more time:
 - Split the core application logic into its own project/library. If you needed to design jobs, tasks, etc. around this points project, that didn't really "belong" to the API, but they needed to run similar logic, you'd probably want that logic pulled out of the API project and put into its own project that could be a library referenced wherever necessary.
 - Add a persistence data source.
 - Add more robust error handling.
 - Add logging.
 - Add some more tests. 
 - Add more documentation.
