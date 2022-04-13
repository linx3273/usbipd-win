﻿// SPDX-FileCopyrightText: 2022 Frans van Dorsselaer
//
// SPDX-License-Identifier: GPL-2.0-only

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Usbipd;

namespace UnitTests;

[TestClass]
sealed class VidPid_Tests
{
    [TestMethod]
    public void DefaultConstructor()
    {
        var vidPid = new VidPid();

        Assert.AreEqual(0, vidPid.Vid);
        Assert.AreEqual(0, vidPid.Pid);
    }

    sealed class VidPidData
    {
        static readonly string[] _Invalid = new[]
        {
            "",
            ":",
            "000:0000",
            "0000:000",
            "00000:0000",
            "0000:00000",
            " 0000:0000",
            "0000 :0000",
            "0000: 0000",
            "0000:0000 ",
            "000g:0000",
            "0000:000g",
        };

        public static IEnumerable<string[]> Invalid
        {
            get => from value in _Invalid select new string[] { value };
        }

        static readonly string[] _Valid = new[]
        {
            "0000:0000",
            "0123:0000",
            "4567:0000",
            "89ab:0000",
            "89AB:0000",
            "cdef:0000",
            "CDEF:0000",
            "0000:0123",
            "0000:4567",
            "0000:89ab",
            "0000:89AB",
            "0000:cdef",
            "0000:CDEF",
            "fFfF:FfFf",
        };

        public static IEnumerable<string[]> Valid
        {
            get => from value in _Valid select new string[] { value };
        }

        static int ExpectedCompare(string left, string right)
        {
            var leftVid = ushort.Parse(left.Split(':')[0], NumberStyles.AllowHexSpecifier);
            var leftPid = ushort.Parse(left.Split(':')[1], NumberStyles.AllowHexSpecifier);
            var rightVid = ushort.Parse(right.Split(':')[0], NumberStyles.AllowHexSpecifier);
            var rightPid = ushort.Parse(right.Split(':')[1], NumberStyles.AllowHexSpecifier);

            return
                leftVid < rightVid ? -1 :
                leftVid > rightVid ? 1 :
                leftPid < rightPid ? -1 :
                leftPid > rightPid ? 1 : 0;
        }

        public static IEnumerable<object[]> Compare
        {
            get => from left in _Valid from right in _Valid select new object[] { left, right, ExpectedCompare(left, right) };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(VidPidData.Invalid), typeof(VidPidData))]
    public void TryParseInvalid(string text)
    {
        var result = VidPid.TryParse(text, out var vidPid);
        Assert.IsFalse(result);
        Assert.AreEqual(0, vidPid.Vid);
        Assert.AreEqual(0, vidPid.Pid);
    }

    [DataTestMethod]
    [DynamicData(nameof(VidPidData.Valid), typeof(VidPidData))]
    public void TryParseValid(string text)
    {
        var result = VidPid.TryParse(text, out var vidPid);
        Assert.IsTrue(result);

        var expectedVid = ushort.Parse(text.Split(':')[0], NumberStyles.AllowHexSpecifier);
        var expectedPid = ushort.Parse(text.Split(':')[1], NumberStyles.AllowHexSpecifier);
        Assert.AreEqual(expectedVid, vidPid.Vid);
        Assert.AreEqual(expectedPid, vidPid.Pid);
    }

    [DataTestMethod]
    [DynamicData(nameof(VidPidData.Invalid), typeof(VidPidData))]
    public void ParseInvalid(string text)
    {
        Assert.ThrowsException<FormatException>(() =>
        {
            var vidPid = VidPid.Parse(text);
        });
    }

    [DataTestMethod]
    [DynamicData(nameof(VidPidData.Valid), typeof(VidPidData))]
    public void ParseValid(string text)
    {
        var vidPid = VidPid.Parse(text);
        var expectedVid = ushort.Parse(text.Split(':')[0], NumberStyles.AllowHexSpecifier);
        var expectedPid = ushort.Parse(text.Split(':')[1], NumberStyles.AllowHexSpecifier);
        Assert.AreEqual(expectedVid, vidPid.Vid);
        Assert.AreEqual(expectedPid, vidPid.Pid);
    }

    [DataTestMethod]
    [DynamicData(nameof(VidPidData.Compare), typeof(VidPidData))]
    public void Compare(string left, string right, int expected)
    {
        var result = VidPid.Parse(left).CompareTo(VidPid.Parse(right));
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DynamicData(nameof(VidPidData.Valid), typeof(VidPidData))]
    public void ToStringValid(string text)
    {
        var vidPid = VidPid.Parse(text);
        var expected = text.ToLowerInvariant();
        var result = vidPid.ToString();
        Assert.AreEqual(expected, result);
    }

    sealed class HardwareIdData
    {
        static readonly string[] _Invalid = new[]
        {
            "",
            "VID_&PID_",
            "VID_000&PID_0000",
            "VID_0000&PID_000",
            "VID_00000&PID_0000",
            "VID_0000&PID_00000",
            "VID_0000 &PID_0000",
            "VID_0000& PID_0000",
            "VID_000g&PID_0000",
            "VID_0000&PID_000g",
            "ID_0000&PID_0000",
            "0000&0000",
            "VID_0000:PID_0000",
            "0000:0000",
        };

        public static IEnumerable<string[]> Invalid
        {
            get => from value in _Invalid select new string[] { value };
        }

        static readonly string[] _Valid = new[]
        {
            "VID_0000&PID_0000",
            "VID_0123&PID_0000",
            "VID_4567&PID_0000",
            "VID_89ab&PID_0000",
            "VID_89AB&PID_0000",
            "VID_cdef&PID_0000",
            "VID_CDEF&PID_0000",
            "VID_0000&PID_0123",
            "VID_0000&PID_4567",
            "VID_0000&PID_89ab",
            "VID_0000&PID_89AB",
            "VID_0000&PID_cdef",
            "VID_0000&PID_CDEF",
            "VID_fFfF&PID_FfFf",
            "xxxVID_0000&PID_0000xxx",
        };

        public static IEnumerable<string[]> Valid
        {
            get => from value in _Valid select new string[] { value };
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(HardwareIdData.Invalid), typeof(HardwareIdData))]
    public void FromHardwareOrInstanceIdInvalid(string text)
    {
        Assert.ThrowsException<FormatException>(() =>
        {
            var vidPid = VidPid.FromHardwareOrInstanceId(text);
        });
    }

    [DataTestMethod]
    [DynamicData(nameof(HardwareIdData.Valid), typeof(HardwareIdData))]
    public void FromHardwareOrInstanceIdValid(string text)
    {
        var vidPid = VidPid.FromHardwareOrInstanceId(text);
        var expectedVid = ushort.Parse(text.Split("VID_")[1][..4], NumberStyles.AllowHexSpecifier);
        var expectedPid = ushort.Parse(text.Split("PID_")[1][..4], NumberStyles.AllowHexSpecifier);
        Assert.AreEqual(expectedVid, vidPid.Vid);
        Assert.AreEqual(expectedPid, vidPid.Pid);
    }

    [TestMethod]
    public void StubValue()
    {
        Assert.AreEqual(0x80EE, VidPid.Stub.Vid);
        Assert.AreEqual(0xCAFE, VidPid.Stub.Pid);
    }
}
