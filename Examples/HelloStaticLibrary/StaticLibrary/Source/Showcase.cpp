#include "StaticLibrary/Showcase.h"

#ifdef _WIN32
#include <Windows.h>
void Showcase()
{
    MessageBox(NULL, TEXT("Hello from StaticLibrary module!"), TEXT("HelloWorldProject"), MB_OK);
}
#else
#include <cstdio>
void Showcase()
{
    std::puts("Hello from StaticLibrary module!");
}
#endif // _WIN32
