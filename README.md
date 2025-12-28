# Movies Web Application Documentation
This document provides an overview of the Blazor.Movies.Web application, its components, and functionality based on the provided source code.

## 1. Introduction
   The Blazor.Movies.Web application is a web application designed to display a list of movies. It is built using the Blazor framework. The application's core functionality involves fetching movie data from a JSON file and making it available for presentation in the user interface.
## 2. Project Structure
   Based on the provided test file, the project likely follows a standard structure for Blazor applications:
   - Blazor.Movies.Web: The main Blazor project.
   - Models: Contains the data models.
      - Movie.cs: Defines the structure for a movie object.
   
   - /Services: Contains services that encapsulate business logic.
      - MovieProvider.cs: Responsible for loading movie data.
   - /StateManager: manages state of data for whole application.

   - /wwwroot: Likely contains static assets, such as the movies.json data file and associated movie images.
   
   - Blazor.Movies.Web.Tests: A separate test project for unit testing the application's components.

## 3. Core Components
   ### 3.1. Models
   - Movie The Movie class represents a single movie entity in the application.
   ```csharp
   public class Movie
   {
   public int Id { get; set; }
   public string Title { get; set; }
   public double Rating { get; set; }
   public string Genre { get; set; }
   public string Image { get; set; }
   }
   ````
   
   
   ### 3.2. Services
   - MovieProvider This service is responsible for data access logic related to movies. It has a dependency on ILogger for logging purposes.
      - Task<Movie[]> LoadMovies(string filePath): This asynchronous method reads a specified JSON file, deserializes its content into an array of Movie objects, and returns it.
   - Error Handling: The provider is robust against file system issues. If the specified file does not exist or is empty, it returns an empty array and logs the event, preventing the application from crashing.

  ### 3.3 StateManager

  ### **Overview**

`StateManager` is a lightweight state management helper.
It provides a simple way to:

* Publish updated state values
* Subscribe to state changes
* Retrieve the **current state** (even for subscribers that initialize *after* the event was last published)

This makes it useful for MVVM applications where multiple components or view models need to share or react to changes in application state.

---

### **Why Use StateManager?**

Prism’s event system is great for broadcasting updates, but:

* It **does not store the latest state**
* Late subscribers **miss previously published events**
* Components often need both **event-driven updates** *and* **current state**

`StateManager` solves this by pairing events with an internal state cache.

---

### **Key Features**

* 🔄 Publish/subscribe 
* 📦 Stores the latest value for each event type
* 🕒 Late subscribers can access the current state immediately
* 🧼 Small, fast, and easy to integrate

---

### **Installation**

Add the `StateManager` and `IStateManager` classes to your project and register them with Prism’s DI container:

```csharp
containerRegistry.RegisterSingleton<IStateManager, StateManager>();
```

---

### **How It Works**

1. **Update**
   Stores the new state and publishes an event.

2. **Subscribe**
   Registers a callback to execute whenever the event publishes.

3. **GetCurrentState**
   Retrieves the cached state for that event type.

The internal dictionary maps each event type to its latest value.

---

### **Usage Examples**

### **Publish a New State**

```csharp
_stateManager.UpdateState(("allmovies", [movie1,movie2]));
```

### **Subscribe to Updates**

```csharp
StateSubscriber.StateEventHandler handler = (state) => {};
_stateManager.Subscribe(("allmovies", handler) action);
});
```
### **Unsubscribe an event**

```csharp
_stateManager.Unsubscribe(("allmovies", handler) action);
});

```

### **Retrieve the Current State**

```csharp
var movies = _stateManager
    .GetCurrentState("allmovies");
```

---


### **Benefits**

* Ensures a consistent state across all components
* Avoids missing state for late subscribers
* Reduces boilerplate for shared state handling



  ## 4. Testing
   The application includes a dedicated test project (Blazor.Movies.Web.Tests) that uses NUnit and Moq for unit testing. The MovieProviderTests class demonstrates a solid testing strategy for the MovieProvider service.
   The tests cover the following scenarios:
   - Happy Path: Successfully loading a valid movies.json file.
   - File Not Found: Handling cases where the JSON file does not exist.
   - Empty File: Handling cases where the JSON file is empty.

   This ensures the MovieProvider is reliable under different conditions.
   Here is a review of the provided test file with some suggestions for improvement.

