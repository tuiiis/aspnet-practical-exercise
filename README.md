# To-do List Application

In this project, you must design and develop a web application according to the requirements that are specified in the task description. You can choose between two architectural approaches based on your skill level:

## Architecture Options

### Option 1: Monolithic MVC Application (Recommended for Regular Students)
  * The application consists of a single [ASP.NET Core MVC application](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) that handles both UI and data processing.
  * The data store must be a relational database management system such as SQL Server Express.
  * To access application data the application must use [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/).
  * The application must use the [ASP.NET Core Identity API](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) to manage users, passwords and profile data.
  * This approach is simpler and allows you to focus on learning ASP.NET Core MVC fundamentals.

### Option 2: 3-Tier Architecture with Separate Web API (Advanced Students Only)
  * The application contains two separate web components - a web application and a web API application.
  * The web application is a [ASP.NET Core MVC application](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) that provides the user interface.
  * The web API application is a [controller-based ASP.NET Core Web API application](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis) that handles data operations.
  * The MVC application communicates with the Web API through HTTP REST calls.
  * The data store must be a relational database management system such as SQL Server Express.
  * To access application data the Web API application must use [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/).
  * The application must use the [ASP.NET Core Identity API](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) to manage users, passwords and profile data.
  * This approach is more complex but teaches distributed application architecture and API design.


## Backlog

The application functional requirements are described in the [Functional Requirements](functional-requirements.md) document.

The [backlog with the user stories](https://en.wikipedia.org/wiki/Product_backlog) you need to implement is given in the table below. The full list of user stories with descriptions is in the [User Stories](user-stories.md) document.

Here are some hints for you:
* Mark stories as completed in the [README.md](README.md) file. Once you've finished developing a user story, mark it as completed in the "Is completed?" column (use the "+" or any other text). This mark will help the mentor who will review your project understand which functionality is implemented and which is not.
* Focus on quality. Implement as many user stories as possible, but if you see that you do not have enough time to complete all user stories, polish the user stories you have already completed.

| Epic | User Story | Description                                                                     | Is completed? |
|------|------------|---------------------------------------------------------------------------------|---------------|
| EP01 | US01       | View the list of my to-do lists.                                                | ✅            |
| EP01 | US02       | Add a new to-do list.                                                           | ✅            |
| EP01 | US03       | Delete a to-do list.                                                            | ✅            |
| EP01 | US04       | Edit a to-do list.                                                              | ✅            |
| EP02 | US05       | View the list of tasks in a to-do list.                                         | ✅            |
| EP02 | US06       | View the task details page.                                                     | ✅            |
| EP02 | US07       | Add a new to-do task.                                                           | ✅            |
| EP02 | US08       | Delete a to-do task.                                                            | ✅            |
| EP02 | US09       | Edit a to-do task.                                                              | ✅            |
| EP02 | US10       | Highlight tasks that are overdue.                                               | ✅            |
| EP03 | US11       | View a list of tasks assigned to me.                                            | ✅            |
| EP03 | US12       | Filter tasks in my assigned task list.                                          | ✅            |
| EP03 | US13       | Sort tasks in my assigned task list.                                            | ✅            |
| EP03 | US14       | Change the status of a task from the list of assigned tasks.                    | ✅            |
| EP04 | US15       | Search for tasks with specified text in the task title.                         |               |
| EP04 | US16       | Highlight tasks that are overdue on the search result page.                     |               |
| EP05 | US17       | View a list of tags on the task details page.                                   |               |
| EP05 | US18       | View a list of all tags.                                                        |               |
| EP05 | US19       | View a list of tasks tagged by a specific tag.                                  |               |
| EP05 | US20       | Add a tag to a task.                                                            |               |
| EP05 | US21       | Remove a tag that is added to a task.                                           |               |
| EP06 | US22       | View the comments on the task details page.                                     |               |
| EP06 | US23       | Add a new comment to the task.                                                  |               |
| EP06 | US24       | Delete a comment that is added to a task.                                       |               |
| EP06 | US25       | Edit a new comment                                                              |               |
| EP07 | US26       | Sign up                                                                         |               |
| EP07 | US27       | Sign in                                                                         |               |
| EP07 | US28       | Sign out                                                                        |               |
| EP07 | US29       | Restore password                                                                |               |
| EP08 | US30       | Application menu                                                                |               |


## Software Architecture

The architecture of the application is described in the [Software Architecture](software-architecture.md) document.


## Solution Requirements

The requirements for the application are described in the [Solution Requirements](solution-requirements.md) document.


## Delivery Plan

The [delivery plan](delivery-plan.md) contains the list of technical tasks distributed over the weeks these tasks must be delivered.


## Project Evaluation

The project is evaluated using both technical evaluation criteria and an assessment of the scope and quality of the implementation of user stories.
