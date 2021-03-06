#include <string>
#include <windows.h>
#include <conio.h>
#include <psapi.h>
#include <string.h>

using namespace std;

bool insertDll(DWORD procID, std::string dll)
{
    //Find the address of the LoadLibrary api, luckily for us, it is loaded in the same address for every process
    HMODULE hLocKernel32 = GetModuleHandle("Kernel32");
    FARPROC hLocLoadLibrary = GetProcAddress(hLocKernel32, "LoadLibraryA");
    
    //Adjust token privileges to open system processes
    HANDLE hToken;
    TOKEN_PRIVILEGES tkp;
    if(OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken))
    {
        LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &tkp.Privileges[0].Luid);
        tkp.PrivilegeCount = 1;
        tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
        AdjustTokenPrivileges(hToken, 0, &tkp, sizeof(tkp), NULL, NULL);
    }

    //Open the process with all access
    HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, procID);

    //Allocate memory to hold the path to the Dll File in the process's memory
    dll += '\0';
    LPVOID hRemoteMem = VirtualAllocEx(hProc, NULL, dll.size(), MEM_COMMIT, PAGE_READWRITE);

    //Write the path to the Dll File in the location just created
    DWORD numBytesWritten;
    WriteProcessMemory(hProc, hRemoteMem, dll.c_str(), dll.size(), &numBytesWritten);

    //Create a remote thread that starts begins at the LoadLibrary function and is passed are memory pointer
    HANDLE hRemoteThread = CreateRemoteThread(hProc, NULL, 0, (LPTHREAD_START_ROUTINE)hLocLoadLibrary, hRemoteMem, 0, NULL);

    //Wait for the thread to finish
    bool res = false;
    if (hRemoteThread)
    {
        WaitForSingleObject(hRemoteThread, INFINITE);
        res = true;
    }

    //Free the memory created on the other process
    VirtualFreeEx(hProc, hRemoteMem, dll.size(), MEM_RELEASE);

    //Release the handle to the other process
    CloseHandle(hProc);

    return res;
}

int main()
{       
    DWORD pid;
    char path[MAX_PATH];
    ZeroMemory(path, MAX_PATH);
        
    HWND hWnd = FindWindowA("TibiaClient", NULL);
    GetWindowThreadProcessId(hWnd, &pid);
    GetCurrentDirectoryA(MAX_PATH, path);
    strcat(path, "\\BynaCam.dll");
    
    if (!insertDll(pid, path))
    {
        MessageBox(0, "Unable to inject DLL into Tibia process!!", "Error!", MB_ICONERROR | MB_OK);
        return -1;
    } else return 0;

    return 0;
}
