## How to Run

```
➜  AkkaNetSample git:(master) ✗ dotnet run  
PreStart: [akka://test/user/first-actor#777862253]
First: [akka://test/user/first-actor#777862253]
Second: [akka://test/user/first-actor/second-actor#1253768516]
PostStop: [akka://test/user/first-actor#777862253]
```

## Questions

 - Can an actor hold a reference to other actors?
 - When an actor creates another actor, what's the lifetime of that actor?