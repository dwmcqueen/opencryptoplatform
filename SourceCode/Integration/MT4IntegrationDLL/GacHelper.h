#include <MSCorEE.h>
#include <windows.h>
#include <fusion.h>
#include <atlstr.h>

typedef HRESULT (__stdcall *CreateAsmCache)(IAssemblyCache**ppAsmCache, DWORD dwReserved);

void InitFunctionPointers();

public __value enum InstallResult {
   OK,
   FAILURE
};

// This is a rather unique managed C++ GAC helper class.
public __gc class GACHelper
{
	// Static constructor.
	static GACHelper()
	{
		InitFunctionPointers();
	}

public:
	static InstallResult InstallAssembly(System::String* fileName);
	static bool QueryAssembly(System::String* fileName);
};