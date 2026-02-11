extern void Print();


#ifdef HELLOWORLD_USE_WIN32_MESSAGEBOX
int CALLBACK WinMain
(
    _In_     HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_     LPSTR     lpCmdLine,
    _In_     int       nCmdShow
)
#else
int main()
#endif // HELLOWORLD_USE_WIN32_MESSAGEBOX
{
    Print();
}
