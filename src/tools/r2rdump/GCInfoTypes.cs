﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection.PortableExecutable;
using System.Text;

namespace R2RDump
{
    class GcInfoTypes
    {
        private Machine _target;

        internal int SIZE_OF_RETURN_KIND_SLIM { get; } = 2;
        internal int SIZE_OF_RETURN_KIND_FAT { get; } = 2;
        internal int CODE_LENGTH_ENCBASE { get; } = 8;
        internal int NORM_PROLOG_SIZE_ENCBASE { get; } = 5;
        internal int SECURITY_OBJECT_STACK_SLOT_ENCBASE { get; } = 6;
        internal int GS_COOKIE_STACK_SLOT_ENCBASE { get; } = 6;
        internal int PSP_SYM_STACK_SLOT_ENCBASE { get; } = 6;
        internal int GENERICS_INST_CONTEXT_STACK_SLOT_ENCBASE { get; } = 6;
        internal int STACK_BASE_REGISTER_ENCBASE { get; } = 3;
        internal int SIZE_OF_EDIT_AND_CONTINUE_PRESERVED_AREA_ENCBASE { get; } = 4;
        internal int REVERSE_PINVOKE_FRAME_ENCBASE { get; } = 6;
        internal int SIZE_OF_STACK_AREA_ENCBASE { get; } = 3;
        internal int NUM_SAFE_POINTS_ENCBASE { get; } = 3;
        internal int NUM_INTERRUPTIBLE_RANGES_ENCBASE { get; } = 1;
        internal int INTERRUPTIBLE_RANGE_DELTA1_ENCBASE { get; } = 6;
        internal int INTERRUPTIBLE_RANGE_DELTA2_ENCBASE { get; } = 6;

        internal int MAX_PREDECODED_SLOTS { get; } = 64;
        internal int NUM_REGISTERS_ENCBASE { get; } = 2;
        internal int NUM_STACK_SLOTS_ENCBASE { get; } = 2;
        internal int NUM_UNTRACKED_SLOTS_ENCBASE { get; } = 1;
        internal int REGISTER_ENCBASE { get; } = 3;
        internal int REGISTER_DELTA_ENCBASE { get; } = 2;
        internal int STACK_SLOT_ENCBASE { get; } = 6;
        internal int STACK_SLOT_DELTA_ENCBASE { get; } = 4;

        internal GcInfoTypes(Machine machine)
        {
            _target = machine;

            switch (machine)
            {
                case Machine.Amd64:
                    SIZE_OF_RETURN_KIND_FAT = 4;
                    NUM_SAFE_POINTS_ENCBASE = 2;
                    break;
                case Machine.Arm:
                    CODE_LENGTH_ENCBASE = 7;
                    SECURITY_OBJECT_STACK_SLOT_ENCBASE = 5;
                    GS_COOKIE_STACK_SLOT_ENCBASE = 5;
                    PSP_SYM_STACK_SLOT_ENCBASE = 5;
                    GENERICS_INST_CONTEXT_STACK_SLOT_ENCBASE = 5;
                    STACK_BASE_REGISTER_ENCBASE = 1;
                    SIZE_OF_EDIT_AND_CONTINUE_PRESERVED_AREA_ENCBASE = 3;
                    REVERSE_PINVOKE_FRAME_ENCBASE = 5;
                    NUM_INTERRUPTIBLE_RANGES_ENCBASE = 2;
                    INTERRUPTIBLE_RANGE_DELTA1_ENCBASE = 4;
                    NUM_STACK_SLOTS_ENCBASE = 3;
                    NUM_UNTRACKED_SLOTS_ENCBASE = 3;
                    REGISTER_ENCBASE = 2;
                    REGISTER_DELTA_ENCBASE = 1;
                    break;
                case Machine.Arm64:
                    SIZE_OF_RETURN_KIND_FAT = 4;
                    STACK_BASE_REGISTER_ENCBASE = 2;
                    NUM_REGISTERS_ENCBASE = 3;
                    break;
                case Machine.I386:
                    CODE_LENGTH_ENCBASE = 6;
                    NORM_PROLOG_SIZE_ENCBASE = 4;
                    SIZE_OF_EDIT_AND_CONTINUE_PRESERVED_AREA_ENCBASE = 3;
                    SIZE_OF_STACK_AREA_ENCBASE = 6;
                    NUM_SAFE_POINTS_ENCBASE = 4;
                    INTERRUPTIBLE_RANGE_DELTA1_ENCBASE = 5;
                    INTERRUPTIBLE_RANGE_DELTA2_ENCBASE = 5;
                    NUM_REGISTERS_ENCBASE = 3;
                    NUM_STACK_SLOTS_ENCBASE = 5;
                    NUM_UNTRACKED_SLOTS_ENCBASE = 5;
                    REGISTER_DELTA_ENCBASE = 3;
                    break;
            }
        }

        internal int DenormalizeCodeLength(int x)
        {
            switch (_target)
            {
                case Machine.Arm:
                    return (x << 1);
                case Machine.Arm64:
                    return (x << 2);
            }
            return x;
        }

        internal int DenormalizeStackSlot(int x)
        {
            switch (_target)
            {
                case Machine.Amd64:
                    return (x << 3);
                case Machine.Arm:
                    return (x << 2);
                case Machine.Arm64:
                    return (x << 3);
            }
            return x;
        }

        internal uint DenormalizeStackBaseRegister(uint x)
        {
            switch (_target)
            {
                case Machine.Amd64:
                    return (x ^ 5);
                case Machine.Arm:
                    return ((x ^ 7) + 4);
                case Machine.Arm64:
                    return (x ^ 29);
            }
            return x;
        }

        internal uint DenormalizeSizeOfStackArea(uint x)
        {
            switch (_target)
            {
                case Machine.Amd64:
                    return (x << 3);
                case Machine.Arm:
                    return (x << 2);
                case Machine.Arm64:
                    return (x << 3);
            }
            return x;
        }

        internal static uint CeilOfLog2(int x)
        {
            if (x == 0)
                return 0;
            uint result = (uint)((x & (x - 1)) != 0 ? 1 : 0);
            while (x != 1)
            {
                result++;
                x >>= 1;
            }
            return result;
        }
    }

    public enum ReturnKinds
    {
        RT_Scalar = 0,
        RT_Object = 1,
        RT_ByRef = 2,
        RT_Unset = 3,       // Encoding 3 means RT_Float on X86
        RT_Scalar_Obj = RT_Object << 2 | RT_Scalar,
        RT_Scalar_ByRef = RT_ByRef << 2 | RT_Scalar,

        RT_Obj_Obj = RT_Object << 2 | RT_Object,
        RT_Obj_ByRef = RT_ByRef << 2 | RT_Object,

        RT_ByRef_Obj = RT_Object << 2 | RT_ByRef,
        RT_ByRef_ByRef = RT_ByRef << 2 | RT_ByRef,

        RT_Illegal = 0xFF
    };

    public enum GcSlotFlags
    {
        GC_SLOT_BASE = 0x0,
        GC_SLOT_INTERIOR = 0x1,
        GC_SLOT_PINNED = 0x2,
        GC_SLOT_UNTRACKED = 0x4,

        // For internal use by the encoder/decoder
        GC_SLOT_IS_REGISTER = 0x8,
        GC_SLOT_IS_DELETED = 0x10,
    };

    public enum GcStackSlotBase
    {
        GC_CALLER_SP_REL = 0x0,
        GC_SP_REL = 0x1,
        GC_FRAMEREG_REL = 0x2,

        GC_SPBASE_FIRST = GC_CALLER_SP_REL,
        GC_SPBASE_LAST = GC_FRAMEREG_REL,
    };

    public class GcStackSlot
    {
        public int SpOffset { get; }
        public GcStackSlotBase Base { get; }
        public GcStackSlot(int spOffset, GcStackSlotBase stackSlotBase)
        {
            SpOffset = spOffset;
            Base = stackSlotBase;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string tab4 = new string(' ', 16);

            sb.AppendLine($"{tab4}SpOffset: {SpOffset}");
            sb.Append($"{tab4}Base: {Enum.GetName(typeof(GcStackSlotBase), Base)}");

            return sb.ToString();
        }
    };
}
