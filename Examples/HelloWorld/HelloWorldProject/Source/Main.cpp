#include <iostream>

#ifdef HELLOWORLD_USE_WIN32_MESSAGEBOX

#include <Windows.h>

int CALLBACK WinMain
(
    _In_     HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_     LPSTR     lpCmdLine,
    _In_     int       nCmdShiw
)
{
    MessageBox(NULL, TEXT("Hello, world!"), TEXT("HelloWorldProject"), MB_OK);
}

#else

int main()
{
    std::cout << "Hello, world!\n";
}

#endif // HELLOWORLD_USE_WIN32_MESSAGEBOX
