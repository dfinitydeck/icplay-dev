using System;
using System.Runtime.InteropServices;
using AOT;
using Framework;
using UnityEngine;

/// <summary>
/// JS call mediator class
/// </summary>
public class JsPlugin : Singleton<JsPlugin>
{
    /// <summary>
    /// Login
    /// </summary>
    /// <returns></returns>
    [DllImport("__Internal", EntryPoint = "login")]
    public static extern string Login();
    
    /// <summary>
    /// Logout
    /// </summary>
    /// <returns></returns>
    [DllImport("__Internal", EntryPoint = "logout")]
    public static extern string Logout();

    /// <summary>
    /// Get current logged in user's principal
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    [DllImport("__Internal", EntryPoint = "getPrincipal")]
    public static extern IntPtr GetPrincipal();


    [DllImport("__Internal", EntryPoint = "freeStringMemory")]
    public static extern void FreeStringMemory(IntPtr ptr);

    /// <summary>
    /// Get balance
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    [DllImport("__Internal", EntryPoint = "getBalance")]
    public static extern int GetBalance(IntPtr principalPtr);

    
    [DllImport("__Internal",EntryPoint = "buy")]
    private static extern void Buy(IntPtr packIdPtr);
    
    [DllImport("__Internal",EntryPoint = "copy")]
    public static extern void Copy(IntPtr ptr);
    
        
    [DllImport("__Internal",EntryPoint = "onLoaded")]
    public static extern void OnLoaded();
    
    public void BuyAsync(IntPtr packIdPtr)
    {
        Buy(packIdPtr);
    }
    
    public static string GetPrincipalString()
    {
        IntPtr ptr = GetPrincipal();
        string result = Marshal.PtrToStringUTF8(ptr);
        FreeStringMemory(ptr);
        return result;
    }

    // /// <summary>
    // /// Purchase package
    // /// </summary>
    // /// <param name="principal"></param>
    // /// <returns></returns>
    // [DllImport("__Internal", EntryPoint = "buy")]
    // public static extern string Buy(string principal);

    public static IntPtr GetIntPtr(string str)
    {
        IntPtr ptr = Marshal.StringToHGlobalAnsi(str);
        return ptr;
    }

    public static string GetStrByIntPtr(IntPtr ptr)
    {
        string result = Marshal.PtrToStringUTF8(ptr);
        FreeStringMemory(ptr);
        return result;
    }
}