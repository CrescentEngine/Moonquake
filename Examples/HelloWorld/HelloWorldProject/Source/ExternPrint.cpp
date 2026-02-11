
#ifdef HELLOWORLD_USE_WIN32_MESSAGEBOX

#include <Windows.h>

void Print()
{
    MessageBox(NULL, TEXT("Hello, world!"), TEXT("HelloWorldProject"), MB_OK);
}

#endif // HELLOWORLD_USE_WIN32_MESSAGEBOX
#include <iostream>

void Print()
{
    std::cout << "Hello, world!\n";
}
