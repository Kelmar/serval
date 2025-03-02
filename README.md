# Moved
Have a new name for the project, and have shifted development over to C++ for the bootstrap compiler.
You can find the new project at [OrangeSoda](https://github.com/Kelmar/OrangeSoda)

# Old

This is currently an experimental language that I'm developing.  That may change in the future.

# Goals
## Syntax
At the moment the syntax has not be solidified for anything.  It will be C/C++/C# like, but may include some useful
constructs from other languages such as Ruby, TypeScript, or D.  While the current code is mostly an experiament,
we need to fully form out the syntax of the language to begin work on the next piece.

### Example
This is an example of the current syntax that is working.

```
import submodule;

class Widget
{
    var foo : int;
}

var bar : Widget;
var baz : int;

baz = 2 + 5 * 10;
```

## Language
* C/C++/C# like syntax
* Easy to interface with hardware at a low level (OS/Driver Development)
  * Pointers & References
  * Pointer arithmetic
  * Structure bit layouts
* Easy to interface with C/C++ (Not sure how yet)
* Stack based objects
* Reflection
* Events
* Lambdas & Closures
* Generics (not templates)
* Interfaces
* Strongly typed
* Const correctness like C++ (read only objects checked at compile time)
* Operator overloading
* Async programming (user space threads (i.e. C# tasks))

## Standard Library
* Common collection library (lists, maps, arrays, etc. with common interface.)
* Thread constructs

## Bootstrap
The current major mile stone is to get a bootstrap compiler working in another language.  This is currently C#, but 
may change depending on requirements in the future.  It is entirely possible this will get replaced with C++ to 
facilitate better portability across platforms and to better interface with LLVM.

