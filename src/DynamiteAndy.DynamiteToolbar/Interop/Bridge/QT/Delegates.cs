using System.Runtime.InteropServices;

namespace DynamiteAndy.DynamiteToolbar.Interop.Bridge.Qt;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ClickedCallbackDelegate(int isChecked);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void TopLevelChangedCallbackDelegate(int topLevel);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void TriggeredCallbackDelegate(int isChecked);