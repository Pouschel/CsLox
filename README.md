# CsLox

Implementation of the virtual machine from [Crafting interpreters](http://craftinginterpreters.com/) in C#, if in doubt consult the [original source](https://github.com/munificent/craftinginterpreters). 

The code is in C# 10 and tries to follow the C-code as close as possible. So it may look  a little bit strange for the seasoned C# programmer.

The commits are named following the chapters. After [chapter 23](http://craftinginterpreters.com/jumping-back-and-forth.html), `CsLoxTester` allows to validate the interpreter using a growing number of original tests.

## Remarks 

* Most of the static functions in C moved to C# classes.
* The `initXXX()` are missing, initialisation is handled in constructors.
* The `Value` union is simplified, containing only a `double` and an `object`; `bool` values are stored as doubles.
* The pointers are replaced with indexes.
* No deallocation code

### Chapter 15

* `PopAndOp` mimics macro `BINARY_OP`

### Chapter 19

* The "struct inheritance" in C is replaces with real inheritance of classes `Obj` and `ObjString`.

### Chapter 20

* The `Table` was implemented using a simple `Dictionary<ObjString,Value>`.

### Chapter 22

* The original `Compiler` is here `CompilerState`, because I used `Compiler` previously.

### Chapter 24

* The native function interface has no `argCount` parameter (we must make an argument copy from the stack, hence the count is obvious).

