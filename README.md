This is currently an experiamental language that I'm developing.  That may change in the future.

# Language Goals
* C/C++/C# like syntax
* Easy to interface with hardware at a low level (OS/Driver Development)
** Pointers & Refrences
** Pointer arithmatic
* Easy to interface with C/C++ (Not sure how yet)
* Stack based objects
* Reflection
* Events
* Lambdas & Closures
* Generics (Not templates)
* Interfaces
* Strongly typed
* Const correctness like C++ (readonly objects checked at compile time)
* Operator overloading

# Standard Library Goals
* Common collection library (lists, maps, arrays, etc. with common interface.)

At the moment the syntax has not be solidified for anything.  It will be C/C++ like, but may include
some useful constructs from other languages such as Ruby, TypeScript, or D.

```c++
class Foo : Bar
{
public:
    Foo()
    {
        // Constructor
    }
    
    ~Foo()
    {
        // Destructor
    }
    
    void DoThing()
    {
        / Doing thing
    }
}

int main(void)
{
	Foo f();
	f.DoThing();
}
```
