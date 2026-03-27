#include "pch.h"
#include "OWL_Native.h"

extern "C" __declspec(dllexport) int Add(int a, int b)
{
    return a + b;
}