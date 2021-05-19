# Simple State Machine Editor and Runtime
This project implements a Visual Studio 2019 extension (.VSIX) and .Net runtime for use on Microsoft Windows. The extension provides a Visual Studio designer for 
building CSharp application-specific event-driven state machines (determinant finite state automata). Runtimes are provided for .Net Framework and .Net Core.

The designer is used to create a code-behind .cs file. This code-behind file defines an abstract class, which contains definitions for the event types, descriptions
of the state transitions, and abstract declarations for each of the actions. The programmer implements a class  which inherits from this abstract class, implementing
actions.
# Why Use the Simple State Machine?
Many coding problems are relatively straight-forward top-to-bottom sequences of steps.  These are easily built using procedural languages like CSharp, Visual Basic, 
C++, Java and so on. Yet some problems are devlishly difficult to implement using simple `if-then-else`, `while-do`, function-oriented programming. Some examples include 
lexical scanners, fast multi-stream asynchronous I/O, and syntax coloring. Trying to do so results in a rats-nest of `if` statements, making the code hard to follow,
difficult to debug, and harder to maintain.

Such problems can sometimes be more easily solved as an event-driven state machine. Such state machines consist of three parts: state definitions, triggering events,
and transitions from one state to another. Procedural actions are associated with each transition, which is where the work gets done. 

A skilled programmer can design and implement a state machine using any number of techniques. Typically, this could include defintions of the events which can take place, 
and a set of `select` statements, one for each state. All this is then encasulated inside a `while` loop. On the other hand, the programmer could define a set of states, and have
methods or `select` statements, one for each event. The .Net event programming model lends itself to such an implementation. 

The first technique makes it easy to ensure that every state considers every possible event, and is, in fact, the model used here. The second technique can be quicker 
to build, but can lead to incomplete state/event mappings. More importantly, both suffer from illegibility. There is no standard way to implement such models, and 
thus each programmer picks their own. In addition, the `select` statements can become very cumbersome and it may be very difficult to see the big picture for future
programmers trying to maintain or enhance the code.

The Simple State Machine Editor endeavors to make it easy for designers to view the overall state machine structure. It provides a convenient mechanism for defining
states, event types, and transitions with their associated actions. It is very easy to change an existing state machine, by adding, moving, or removing states, 
event types and transitions. It's easy to name objects and associate textual descriptions with them. The code-behind file is automatically generated (and regenerated, as needed)
and provides an easy structure for implementing action methods. Finally, the runtime provides an efficient, light-weight implementation, together with useful debugging
aids.
# Building
This software was developed on Microsoft Windows 10 using Visual Studio 2019. Download the files or clone the repository, then open the SimpleStateMachine.sln solution file from Visual Studio. Simply
select Build Solution or press F6.

The extension, SimpleStateMachineEditor.VSIX, will be found under SimpleStateMachineEditor\SimpleStateMachineEditor\bin. 

The .Net Framework runtime, SimpleStateMachine.dll, will be found under SimpleStateMachineEditor\SimpleStateMachine\bin.

The .Net Core runtime, SimpleStateMachineCore.dll, will be found under SimpleStateMachineEditor\SimpleStateMachineCore\bin.
# Installation
The Simple State Machine Editor consists of two pieces: a Visual Studio extension (SimpleStateMachineEditor.VSIX) and a runtime (.Net Framework: SimpleStateMachine.dll, 
.Net Core: SimpleStateMachineCore.dll).

The extension can be installed from the Visual Studio Marketplace, or by building the extension and opening it (either by double-clicking or by the Open command on the right-click context 
menu from the file explorer).

The runtime is available as a Nuget package, SimpleStateMachine.
# Using the Designer
The designer is a graphical representation of the state machine. Below is the designer window for the HelloWorld state machine.
![Image of Hello World designer window](https://SimpleStateMachineEditor.github.com/SimpleStateMachineEditor/Images/HelloWorld_Designer.jpg)
The red box highlights the designer window. States are denoted by pink circles (normal) and red (error) and green (finish) hexagons. Event types are blue lightening bolts. Notice the transitions are shown by arrows joining two states. The event that triggers the transition is shown above the arrow, and the actions executed when the transition is taken are below. 
## Design Window
## Code Window
## Code Behind Window
# Using the Runtime
## Posting Events
## Implementing Actions
## Debugging
# Changing a State Machine
# Samples
## Hello World
##L exical Analyzer
## Mouse Tracking
# Known Issues
## Designer
1. Right-clicking on the editor pane, or directly on an icon, should bring up a context menu near where you click. However, sometimes the context menu shows up at the very top-left corner of 
the window. The problem appears randomly, however once it starts, it remains for the life of the window. A work-around is to close and reopen the .SFSA designer window.
## Runtime
No known issues.
# Support
Report issues, questions, and suggestions through Github. 
# License
Code Project Open License (CPOL) 1.02
# Contributors
- Jeff East (jeffeast@outlook.com)