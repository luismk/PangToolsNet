﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PangyaGameGuardAPI
{
    public static class Utils
    {
        public static string HexDump(this byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            var bytesLength = bytes.Length;

            var HexChars = "0123456789ABCDEF".ToCharArray();

            var firstHexColumn =
                8 // 8 characters for the address
                + 3; // 3 spaces

            var firstCharColumn = firstHexColumn
                                  + bytesPerLine * 3 // - 2 digit for the hexadecimal value and 1 space
                                  + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                                  + 2; // 2 spaces 

            var lineLength = firstCharColumn
                             + bytesPerLine // - characters to show the ascii value
                             + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            var line = (new string(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            var expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            var result = new StringBuilder(expectedLines * lineLength);

            for (var i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                var hexColumn = firstHexColumn;
                var charColumn = firstCharColumn;

                for (var j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        var b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = b < 32 ? '·' : (char)b;
                    }

                    hexColumn += 3;
                    charColumn++;
                }

                result.Append(line);
            }

            return result.ToString();
        }
    }
    // The class that does the marshaling. Making it generic is not required, but
    // will make it easier to use the same custom marshaler for multiple array types.
    public class ArrayMarshaler<T> : ICustomMarshaler
    {
        // All custom marshalers require a static factory method with this signature.
        public static ICustomMarshaler GetInstance(String cookie)
        {
            return new ArrayMarshaler<T>();
        }

        // This is the function that builds the managed type - in this case, the managed
        // array - from a pointer. You can just return null here if only sending the 
        // array as an in-parameter.
        public Object MarshalNativeToManaged(IntPtr pNativeData)
        {
            // First, sanity check...
            if (IntPtr.Zero == pNativeData) return null;
            // Start by reading the size of the array ("Length" from your ABS_DATA struct)
            int length = Marshal.ReadInt32(pNativeData);
            // Create the managed array that will be returned
            T[] array = new T[length];
            // For efficiency, only compute the element size once
            int elSiz = Marshal.SizeOf<T>();
            // Populate the array
            for (int i = 0; i < length; i++)
            {
                array[i] = Marshal.PtrToStructure<T>(pNativeData + sizeof(int) + (elSiz * i));
            }
            // Alternate method, for arrays of primitive types only:
            // Marshal.Copy(pNativeData + sizeof(int), array, 0, length);
            return array;
        }

        // This is the function that marshals your managed array to unmanaged memory.
        // If you only ever marshal the array out, not in, you can return IntPtr.Zero
        public IntPtr MarshalManagedToNative(Object ManagedObject)
        {
            if (null == ManagedObject) return IntPtr.Zero;
            T[] array = (T[])ManagedObject;
            int elSiz = Marshal.SizeOf<T>();
            // Get the total size of unmanaged memory that is needed (length + elements)
            int size = sizeof(int) + (elSiz * array.Length);
            // Allocate unmanaged space. For COM, use Marshal.AllocCoTaskMem instead.
            IntPtr ptr = Marshal.AllocHGlobal(size);
            // Write the "Length" field first
            Marshal.WriteInt32(ptr, array.Length);
            // Write the array data
            for (int i = 0; i < array.Length; i++)
            {   // Newly-allocated space has no existing object, so the last param is false
                Marshal.StructureToPtr<T>(array[i], ptr + sizeof(int) + (elSiz * i), false);
            }
            // If you're only using arrays of primitive types, you could use this instead:
            //Marshal.Copy(array, 0, ptr + sizeof(int), array.Length);
            return ptr;
        }

        // This function is called after completing the call that required marshaling to
        // unmanaged memory. You should use it to free any unmanaged memory you allocated.
        // If you never consume unmanaged memory or other resources, do nothing here.
        public void CleanUpNativeData(IntPtr pNativeData)
        {
            // Free the unmanaged memory. Use Marshal.FreeCoTaskMem if using COM.
            Marshal.FreeHGlobal(pNativeData);
        }

        // If, after marshaling from unmanaged to managed, you have anything that needs
        // to be taken care of when you're done with the object, put it here. Garbage 
        // collection will free the managed object, so I've left this function empty.
        public void CleanUpManagedData(Object ManagedObj)
        { }

        // This function is a lie. It looks like it should be impossible to get the right 
        // value - the whole problem is that the size of each array is variable! 
        // - but in practice the runtime doesn't rely on this and may not even call it.
        // The MSDN example returns -1; I'll try to be a little more realistic.
        public int GetNativeDataSize()
        {
            return sizeof(int) + Marshal.SizeOf<T>();
        }
    }
}
