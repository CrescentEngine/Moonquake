#include "StaticLibrary/Showcase.h"

#ifdef _WIN32
#include <Windows.h>
int CALLBACK WinMain
(
    _In_     HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_     LPSTR     lpCmdLine,
    _In_     int       nCmdShow
)
#else
int main()
#endif // _WIN32
{
    Showcase();
}

