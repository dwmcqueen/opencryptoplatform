// Code allows the usage of the Fusion.dll operations, GAC manipulations, by managed C++ code.
// Based on: http://www.codeguru.com/columns/kate/print.php/c12793 and http://support.microsoft.com/kb/317540

#include "gachelper.h"

HMODULE g_FusionDll;
CreateAsmCache g_pfnCreateAssemblyCache;

void InitFunctionPointers()
{
   LoadLibraryShim(L"fusion.dll", 0, 0, &g_FusionDll);
   g_pfnCreateAssemblyCache = (CreateAsmCache)GetProcAddress(g_FusionDll, "CreateAssemblyCache");
}

// Install assembly.
InstallResult GACHelper::InstallAssembly(System::String* fileName)
{
	CComPtr<IAssemblyCache> pCache = NULL;
	HRESULT hr = g_pfnCreateAssemblyCache(&pCache, 0);

	if (hr == S_OK)
	{
		const __wchar_t __pin * wFileName =
		PtrToStringChars(fileName);
		hr = pCache->InstallAssembly(0, wFileName, NULL);
		if (hr == S_OK)
		{
			return InstallResult::OK;
		}
		System::Runtime::InteropServices::Marshal::ThrowExceptionForHR(hr);
	}

	return InstallResult::FAILURE;
}

// Query installed assembly.
bool GACHelper::QueryAssembly(System::String* fileName)
{
	CComPtr<IAssemblyCache> pCache = NULL;
	HRESULT hr = g_pfnCreateAssemblyCache(&pCache, 0);

	if (hr == S_OK)
	{
		const __wchar_t __pin * wFileName = PtrToStringChars(fileName);
		hr = pCache->QueryAssemblyInfo(0, wFileName, NULL);

		return (hr == S_OK);
	}
	return InstallResult::FAILURE;
}
