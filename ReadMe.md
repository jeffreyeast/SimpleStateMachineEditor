# Simple State Machine Editor and Runtime
This project implements a Visual Studio 2019 extension (.VSIX) and .Net runtime for use on Microsoft Windows. The extension provides a Visual Studio designer for 
building CSharp application-specific event-driven state machines (deterministic finite state automata). Runtimes are provided for .Net Framework and .Net Core.

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

You may have an issue with getting it to build on a machine that has never built a Visual Studio extension before. You need to have chosen the *Visual Studio Extension development* option (under *Other toolsets*) when you installed Visual Studio. 
Run the Visual Studio Installer and ensure this option is checked.

Ensure you have the following extension installed (*Extensions\Manage Extensions*):
- Extensibility Essentials 2019

The solution explicitly references the following Nuget packages. These should be automatically restored for you during the build process:
- Template Builder

If you find it still won't build, try creating a new solution with a VSIX project and build that. Then go back and try to rebuild the SimpleStateMachineEditor. There's some black magic going on behind the scenes, and I found this helped on my machine. If all
else fails, try adding the following lines to the SimpleStateMachineEditor.csproj file:

    <ItemGroup>
        <PackageReference Include="Microsoft.VisualStudio.SDK" Version="16.0.206" ExcludeAssets="runtime" />
        <PackageReference Include="Microsoft.VSSDK.BuildTools" Version="16.10.1055" />
    </ItemGroup>

# Installation
The Simple State Machine Editor consists of two pieces: a Visual Studio extension (SimpleStateMachineEditor.VSIX) and a runtime (.Net Framework: SimpleStateMachine.dll, 
.Net Core: SimpleStateMachineCore.dll).

The extension can be installed from the Visual Studio Marketplace, or by building the extension and opening the SimpleStateMachineEditor.vsix file (either by double-clicking on it, or by right-clicking and selecting the Open command using file explorer).

The runtime is available as a Nuget package, SimpleStateMachine.
# The Designer
The designer is a graphical representation of the state machine. Here is the designer window for the HelloWorld state machine.

![Image of Hello World designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/HelloWorld_Designer.png)

The red box highlights the designer window.  

The green box is the solution explorer. Notice a new file extension: .sfsa ("simple finite state automaton"). The .sfsa file holds the definition of the state machine. It's displayed graphically in the designer window. You can view the underlying code by pressing F7 (View/View Code).  Notice if you expand the "HelloWorld.sfsa" item, you can see the underlying code behind file. This file is automatically generated by the
designer and holds the C# description of the state machine. You seldom, if ever, need to view this this (and should never edit it directly), but it can be interesting to look at.

The orange box surrounds a new toolbar (Simple State Machine Toolbar) provided by the extension. This includes convenient buttons for creating state machine objects, aligning them, and so on.

The purple box surrounds the Actions Tool Window.  This window lists all the actions used in transitions. 

To add a Simple State Machine item to a project, select "Add/New item..." from the project's short cut menu. The template is the Simple State Machine Template. Click on it, enter whatever you like for the filename and click OK. This will create a .sfsa file containing a *Start* state and a wilcard *** event type.  The designer window will appear. You may rename the *Start* state to whatever you like (or delete it entirely). The event type *** has special meaning: when used as a transition trigger, it means "match any event not matched by another transition from this state". 
## Design Window
The designer window is shown below:

![Image of designer window](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/HelloWorld_Designer_Window.png)

States are denoted by pink circles (normal) and green (finish) and red (error) hexagons. Two states are shown ("Start" and "Done"), both surrounded by blue.  Event types are blue lightening bolts. The "SayHelloWorld" event type is surrounded by a red box. Notice transition are arrows joining two states. The event which triggers the transition is shown above the arrow, and the actions executed when the transition is taken are below. The transition between Start and Done is surrounded by a green box in the picture. The trigger event is the "SayHelloWorld" event type, and the action to be taken is a method called `DoSayHello`. 

The diagram also shows a standard search box at the top (surrounded by purple) and a zoom control in the lower right corner (surrounded by yellow).

The designer makes use of three windows and a toolbar. The designer window, the Actions tool window and the Properties (F4) tool window. 

The designer window shows the states, event types and state transitions. The Actions tool window shows the methods which can be referenced by state transitions. The Properties window shows characteristics for the currently selected object.

To select an object, click on a state icon, event type icon or transition icon. Hold the shift key down while clicking on an icon to select multiple objects. Each object has a short cut menu. Right click an object to open its short cut menu. You reposition an object within the window by clicking and dragging it.

Event types and states have names, which can be set from the Properties window. They also have optional textual descriptions. You can provide anything you find useful. 

To create a state, you may select the *Add a new state* button from the toolbar, select the *Tools/State Machine/Add a new state* command, or right click anywhere on the designer window (except over an icon) and select *Add a new state*. Event types are created using a similar mechinism. 

To create a transition, right click on a state to show its short cut menu, and click on *Add a transition*. Then drag the arrow over the transition's terminal (end) state and click it. The designer will draw an arrow from the intial to the ending state.  To assign a trigger event, drag an event icon and drop it on the arrow. 

The Actions tool window holds a WPF DataGrid control  listing all the actions defined for the state machine. You define, edit and destroy action methods in this DataGrid. Click on a row to select it, double-click to edit a name, click followed by the *Delete* key to remove an action. To create a new action method, enter its name into the bottom (blank) row and press the *Enter* key.

To add an action to a transition, click and hold the left mouse button on the action's name in the  Actions Tool Window, and drag it over the transition, then release (drop) the mouse button. To remove an action, do it again. Each transition can have a single, multiple or no actions. You control the relative order of actions within a transition by where you drop the action name, relative to the existing actions associated with the transition. Try it, it's easier than it sounds!

Deleting an object is performed either from its short cut menu, or by selecting the object and clicking the *Remove* button on the toolbar.

Rename a state or event type by selecting it's icon (by clicking on it), then editting its name in the Properties tool window.

States have a "state type": Normal, Finish or Error.  States default to Normal. You can change a state's type from the Properties tool window. Finish and Error states are terminal states: the state machine will cease execution once it enters such a state.

Every state machine must include exactly one start state. This is the state where execution begins.

### Toolbar
The Simple State Machine Toolbar is a standard Visual Studio toolbar. By default, it includes button to create objects (e.g., states, event types), remove existing events, and adjust the position of icons.
### Action Methods and the Actions Tool Window
The Actions Tool Window is only visible when a Simple State Machine Designer window is active. It lists the names of all the action methods defined for the currently active designer window.  You use this window to maintain the list of action methods.
| To... | Do this... |
| ----- | ---------- |
| Define a new action method       | Click on the name cell of the bottom row of the list and enter the method name (must be a legal C# name). Press *Enter* when done. |
| Associate an action with a transition  | Drag the action method name from the Actions Tool Window over the transition's arrow and drop it. |
| Remove an action from a transition | Right click the action under the transition's arrow to bring up its short cut menu and click on *Remove*. |
| Change the order of actions within a transition | Select the action under the transition's arrow and drag it to its new position. |
| Delete an existing action method | Select the row of the target method and press the *Delete* key. |
| Change a method name             | Click on the name cell of the target method and press F2. Alternatively, double click the name cell. Enter the new name and press *Enter* when finished.|
| Enter descriptive text           | Click on the description cell of the target method and press F2. Alternatively, double click the description cell. Enter the text and press *Enter* when finished. |


### Display Layers
Complex state machines can be difficult to view in the designer. Too many states, too many event types and too many transitions. Consider the sample Lexical Analyer's initial designer view:

![Image of Lexical Analyzer designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/LexicalAnalyzerDefaultLayer.png)

It is hard to distinguish exactly what is going on.  But the portion of the state machine dedicated to a particular lexeme type is easier to follow. Here is a view of the states involved in scanning string tokens:

![Image of Lexical Analyzer designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/LexicalAnalyzerStringLayer.png)

The designer gives you the ability to group states into *layers*. Each layer can be viewed independently.  The *Default* layer (*Layer1*) always includes all the states. You can create as many layers as you wish. Each state is always included in the default layer, but can also be a member of as many other layers as you find useful. 

The layer icons are shown at the bottom right corner of the designer window.  The highlighted icon identifies the layer currently being displayed in the designer.

| To... | Do this... |
| ----- | ---------- |
| Create a layer | Click on the *+* sign layer icon (to the right of the other layer icons) |
| Show a layer | Click on the target layer's icon |
| Remove a layer | Right click the layer's icon and choose *Remove* from the short cut menu |
| Add a state to a layer | Drag the state to the target layer's icon |
| Add multiple states to a layer | Select the states and drag one over the target layer's icon |
| Remove a state from a layer | Drag the state to the target layer's icon |
| Rename a layer | Select the layer by clicking on it, then change it's name in the Properties window |
| View the icons in a layer | Hover the mouse over the target layer's icon. The member states icons will be highlighted |

Event types are not layered -- every event type shows in every layer. This allows you to reference any event type in any layer view.

### Groups
Another tool at your disposal for simplifying state machine diagrams is the *group*. A *group* is a collection of inter-related states which are displayed together as a group icon, rather than individual state
icons. Each group has a layer associated with it. Opening the group's layer displays its constituent state icons, together with their transitions. 
So in the default layer, you see the group icon, but in the group's layer, you see the states within the group.
The idea is that the transitions between the members of the group can be ignored when viewing the big picture of the overall state machine. You manipulate the members of the group from within it's associated layer.

Below is the lexical analyzer after its states have been moved to appropriate groups.

![Image of Lexical Analyzer designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/LexicalAnalyzerGroupsLayer.png)

| To... | Do this... |
| ----- | ---------- |
| Create a group | Right click from within the designer window and choose *Add new group* |
| View the states within a group | Click on the icon for the layer associated with the group. |
| Go back to the default layer | Click on the default layer's icon |
| Remove a group | From the default layer, right click on the group's icon and choose *Remove* |
| Add a state to a group from the default layer | Drag the state onto the group icon or the icon for it's associated layer |
| Add a state to a group from within the group's associated layer | Right click from within the designer window and choose *Add new state* |
| Remove a state from a group | From within the window for the group's associated layer, drag the state's icon onto the layer icon |

When you open a group's layer, you will see the states within the group as well as their transitions.
You will also see all the event types associated with the state machine -- event types are not members of a group, they are common
across the entire state machine. This lets you create transitions within the group for any event type. 

This image is of the *Strings* group within the lexical analyzer.

![Image of Lexical Analyzer designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/LexicalAnalyzerStringLayer.png)

Opening a group, you will see the states you have added to the group, and you will also see states which share transitions between those states. This allows you to view the transitions affecting the states in the 
group. Explicitly added states have solid backgrounds, implicit members are cross-hatched. You can fully manipulate the explicitly added states (move them, rename them, add transitions, even remove them from the 
group or delete them). Actions are limited for implicit members: you can move and rename them, add and remove transitions. But you cannot remove them -- their presence is automatically maintained by the designer,
and is based solely on transitions they share with explicit group members.

Note that a state can only belong to one group.

### Hovering
The designer makes extensive use of the mouse cursor "hovering" over an icon. It highlights related objects while the mouse is hovering over an icon. For example, hovering over an event type icon will highlight the transitions whose trigger is that event type.

| Hovering over... | Highlights ... |
| ---------------- | --------------- |
| State | The transitions which originate from that state |
| Event type | The transitions whose trigger event is the event type |
| Transition | The transition and its starting and ending states |
| Method name in the Actions Tool Window | All transiton actions where the action is used |
| Layer | All members of the layer |
| Group | Members of the group visible from the current layer |

### Search
The designer window includes a standard Visual Studio search box at the top of the window. You use this to search state and event type names. Matching names are highlighted in the designer.
## Code Window
The Simple State Machine definition is persisted as an XML file.  You usually do not need to view this file, so it is not displayed by default. Pressing F7 (or *View/View code*) will display this file.

You must exercise great care if you edit the XML file directly. Invalid constructs may cause the designer not to operate correctly.

## Code Behind File
The runtime description of the state machine is found in a C# code-behind file. This file is automatically created by the designer at build time. 

The code-behind file defines an abstract class which defines the state machine. This includes
- The abstract class
- The event types
- Abstract declarations of each action method

For example, the code-behind file for Hello World is:

    // ------------------------------------------------------------------------------
    // <auto-generated>
    //     This file was generated by SfsaGenerator
    // </auto-generated>
    // ------------------------------------------------------------------------------

    namespace HelloWorld
    {
        using System;
        using System.Collections.Generic;
        using SimpleStateMachine;

        public abstract class HelloWorldStateMachine : StateMachineWithoutReturnValueBase
        {
            public enum EventTypes
            {
                SayHelloWorld,
            };

            static readonly string[] EventTypeNames = new string[]
            {
                "SayHelloWorld",

            };

            static readonly string[] StateNames = new string[]
            {
                "Done",
                "Start",

            };

            protected override int StartState => Array.IndexOf(StateNames, "Start");

            static readonly StateTypes[] StateClassifications = new StateTypes[]
            {

                StateTypes.Finish,
                StateTypes.Normal,
            };

            /// <summary>
            /// Action Routines
            /// 
            /// You must override each of these action routines in your implementation.
            /// </summary>

            protected abstract void DoSayHello();


            protected override Transition<Action>[,] Transitions => _transitions;
            Transition<Action>[,] _transitions;

            public HelloWorldStateMachine() : base (StateClassifications, EventTypeNames, StateNames)
            {
                _transitions = new Transition<Action>[,]
                {
                    { // Done(0)
                        new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // SayHelloWorld(0)
                    },
                    { // Start(1)
                        new Transition<Action>(0, new Action[] { DoSayHello, }),  // SayHelloWorld(0)
                    },
                };
            }

            /// <summary>
            /// Invoked to execute the state machine.
            /// </summary>
            /// <param name="e">Provides an optional event type to post at normal priority before starting execution</param>

            /// <exception cref="System.InvalidOperationException">Thrown if an event is chosen for 
            /// execution and no transition from the current state maches the event.
            /// </exception>
            /// <remarks>
            /// The state machine runs until one of the following conditions is met:
            /// - There are no remaining events to process
            /// - A stop or error state is entered
            /// - An event is encountered and no transition matches the event type
            /// - An action raises an exception
            /// For each state, the next event to be processed is chosen from the head of the
            /// internal event queue, and if no event is found, then the external event queue.
            /// </remarks>
            public void Execute(EventTypes? e = null)
            {
                 base.Execute(e.HasValue ? (int)e.Value : default(int?));
            }

            /// <summary>
            /// Invoked by an action routine to post an internal (high-priority) event.
            /// <param name=eventType>Identifies the event to be posted</param>
            /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
            /// </summary>
            protected void PostHighPriorityEvent(EventTypes eventType)
            {
                PostHighPriorityEvent((int)eventType);
            }

            /// <summary>
            /// Invoked by any code to post an external (lower-priority) event.
            /// <param name=eventType>Identifies the event to be posted</param>
            /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
            /// </summary>
            public void PostNormalPriorityEvent(EventTypes eventType)
            {
                PostNormalPriorityEvent((int)eventType);
            }
        }
    }


# Using the Runtime
Once you have defined the Simple State Machine, you need to implement a class based on the abstract class declared in the code-behind file. This class implements the abtract action methods declared in the abstract class. It may also implement additional methods, for example, one that posts the initial event to the state machine and causes the state machine to execute.

The state machine itself if a passive declaration -- it will do nothing until 1) at least one event is posted to it, and 2) its `Execute` method is invoked. 

The execution loop for a state machine is very simple. It starts by invoking the state machine's `Execute` method. The `Execute` method is a synchronous method and runs in a loop until either until there are no events to process or the *current state* is a *Finish* or *Error* state.

The machine has very little contextual state built into it:
- The state transition diagram
- The *current state*
- Two FIFO event lists: a high-priority list and a normal-priority list.

The loop begins at the *current state*. If this state's type is *Finish* or *Error*, the loop terminates. Otherwise, the machine removes the event at the head of the high-priority event list. If this list is empty, it removes the event at the head of the normal-priority list. If this list is also empty, the loop terminates and control returns to the caller of `Execute`.

The state transitions from the *current state* are examined for one whose trigger event type matches the selected event. If no transition matches and there is no wildcard trigger, the machine terminates by throwing the `UnexpectedEventException` exception. Otherwise, the action methods associated with the transition are executed serially. Once the last action returns, the *current state* is set to the transition's end state and the loop repeats.

The sample Hello World program that invokes the state machine is:

    using System;

    namespace HelloWorld
    {
        class Program
        {
            static void Main(string[] args)
            {
                HelloWorldActions stateMachine = new HelloWorldActions();
                stateMachine.Execute(HelloWorldStateMachine.EventTypes.SayHelloWorld);
            }
        }
    }


## Posting Events
The Simple State Machine implements two priorities of events: high- and normal-priority. Events within a priority are processed in the order received. All the waiting high-priority events are processed before any normal-priority event.

The motivation for the two priorities is to simplify state machine design. It promotes a design pattern whereby an action is invoked which performs some test and posts its result as a high-priority event. This allows the transition's end state to limit its own transitions to those event types which this action produces: for example, *Yes* or *No*; *Is* or *IsNot*. Such states can ignore the presence of normal-priority events, because it is certain to see only the high-priority event posted by the action performing the test. This greatly simplifies design patterns in a program where normal-priority events may arrive in groups or asychronous to machine execution.

Two methods are provided for posting events to the state machine:  `PostHighPriorityEvent` and `PostNormalPriorityEvent`. Generally, `PostHighPriorityEvent` is only invoked by internal action methods. `PostNormalPriorityEvent` is usually invoked by methods outside the state machine.

Remember, simply posting an event will not cause the machien to run -- it just queues the event for future processing. The `Execute` method must be invoked to start processing. For convenience, you can post a normal-priority event and begin execution using the `Execute(EventTypes e)` method.

## Implementing Actions
Actions are implemented as overrides of the abstract action methods declared in the code-behind file. 

Generally, action methods should be very simple and very straight-forward. An action may post an event by invoking `PostHighPriorityEvent` or `PostNormalPriorityEvent`. However, there is no requirement to do so. Action methods frequently access state in the implementation class. Action methods execute on the same thread as that of the `Execute` method. They execute in the order they appear in the transition. Action methods may raise exceptions that are thrown beyond the method body -- however, the `Execute` method must not be invoked before first calling `Reset`.

The sample implementation class for Hello World is:

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace HelloWorld
    {
        /// <summary>
        /// The implementation of the HelloWorldStateMachine actions
        /// </summary>
        public class HelloWorldActions : HelloWorld.HelloWorldStateMachine
        {
            protected override void DoSayHello()
            {
                Debug.WriteLine("Hello World!");
            }
        }
    }

## Debugging
The most likely error made in state machine design is encountering an event at runtime for which the current state does not have a matching transition. The Simple State Machine runtime detects such occurances and throws `UnexpectedEventException`.
The state machine class includes a `Trace` property which shows the most recent state transitions and the event types which triggered the transitions. 

# Changing a State Machine
The designer is used to create the initial definition of a Simple State Machine as well as for editting it. A common change may be simply adding transitions or changing the end state of an existing transition. Such changes may have no impact on user-code -- they are simply reflected as changes in the generated code-behind file.

Changing the name of an action method, adding new action methods and removing existing action methods all require changes to the action implementation class.
The C# compiler helps by identifying which methods remain to be implemented and those which were removed or renamed.

Generally, maintainers should find it easier to understand a state machine pictorially (through the designer), than if the state machine was simply implemented as a set of ad-hoc `select` statements. Implementors who make judicious use of the *Description* properties of states and event types can make future maintenance faster, easier and less error-prone.
# Samples
## Hello World
The Hello World sample is a very simple example of using the Simple State Machine designer and runtime. The state machine consists of two states (*Start* and *Done*) with one event type (*SayHelloWorld*).
A transition connects the two states and executes the *DoSayHello* action.

This example demonstrates creating a state machine and it's implementation class, then posting an event and executing the state machine.

![Image of Visual Studio with the designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/HelloWorld.png)

## Lexical Analyzer
The Lexical Analyzer sample is a simple lexeme scanner. It scans text and produces lexemes representing tokens. It recognizes identifiers, numbers, strings and punctuation, while ignoring white space and comments.

![Image of Lexical Analyzer designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/LexicalAnalyzerDefaultLayer.png)

## Mouse Tracking
The MouseSelection state machine handles all the mouse events for the designer window. This includes object selection, dragging objects and invoking short cut menus. The source is included in the SimpleStateMachineEditor project. The class implementation file is 
SimpleStateMachineEditor/MouseStateMachine/DesignerMouseSelectionImplementation.cs.

![Image of Visual Studio with the designer](https://github.com/jeffreyeast/SimpleStateMachineEditor/blob/master/SimpleStateMachineEditor/Images/MouseSelection.png)

# Known Issues
## Designer
1. The Action Tool Window is only visible when a .SFSA designer window is active.  This can make placing it problematic when it's a member of a window group and you want to adjust it's position or move it to another group.
An easy solution is to float the window, then position it in the desired window group.
## Runtime
No known issues.
# Support
Report issues, questions, and suggestions through Github. 
# License
Code Project Open License (CPOL) 1.02
# Contributors
- Jeff East (jeffeast@outlook.com)