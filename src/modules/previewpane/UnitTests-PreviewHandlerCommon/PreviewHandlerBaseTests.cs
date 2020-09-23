﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Drawing;
using Common;
using Common.ComInterlop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests_PreviewHandlerCommon
{
    [TestClass]
    public class PreviewHandlerBaseTests
    {
        private static IPreviewHandlerControl previewHandlerControl;

        public class TestPreviewHandler : PreviewHandlerBase
        {
            public TestPreviewHandler()
            {
                Initialize();
            }

            public override void DoPreview()
            {
                throw new NotImplementedException();
            }

            protected override IPreviewHandlerControl CreatePreviewHandlerControl()
            {
                return GetMockPreviewHandlerControl();
            }
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldCallPreviewControlSetWindow_WhenSetWindowCalled()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            var handle = new IntPtr(5);
            var bounds = GetRectangle(2, 2, 4, 4);

            var actualHandle = IntPtr.Zero;
            var actualBounds = Rectangle.Empty;
            mockPreviewControl
                .Setup(_ => _.SetWindow(It.IsAny<IntPtr>(), It.IsAny<Rectangle>()))
                .Callback<IntPtr, Rectangle>((hwnd, rect) =>
                {
                    actualHandle = hwnd;
                    actualBounds = rect;
                });

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();

            // Act
            testPreviewHandler.SetWindow(handle, ref bounds);

            // Assert
            Assert.AreEqual(actualHandle, handle);
            Assert.AreEqual(actualBounds, bounds.ToRectangle());
            mockPreviewControl.Verify(_ => _.SetWindow(It.IsAny<IntPtr>(), It.IsAny<Rectangle>()), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldCallPreviewControlSetrect_WhenSetRectCalled()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            var bounds = GetRectangle(2, 2, 4, 4);

            var actualBounds = Rectangle.Empty;
            mockPreviewControl
                .Setup(_ => _.SetRect(It.IsAny<Rectangle>()))
                .Callback<Rectangle>((rect) =>
                {
                    actualBounds = rect;
                });

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();

            // Act
            testPreviewHandler.SetRect(ref bounds);

            // Assert
            Assert.AreEqual(actualBounds, bounds.ToRectangle());
            mockPreviewControl.Verify(_ => _.SetRect(It.IsAny<Rectangle>()), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldCallPreviewControlUnload_WhenUnloadCalled()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();

            // Act
            testPreviewHandler.Unload();

            // Assert
            mockPreviewControl.Verify(_ => _.Unload(), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldCallPreviewControlSetBackgroundColor_WhenSetBackgroundColorCalled()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            var color = default(COLORREF);

            // Act
            testPreviewHandler.SetBackgroundColor(color);

            // Assert
            mockPreviewControl.Verify(_ => _.SetBackgroundColor(It.Is<Color>(c => (c == color.Color))), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldCallPreviewControlSetTextColor_WhenSetTextColorCalled()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            var color = default(COLORREF);

            // Act
            testPreviewHandler.SetTextColor(color);

            // Assert
            mockPreviewControl.Verify(_ => _.SetTextColor(It.Is<Color>(c => (c == color.Color))), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldCallPreviewControlSetFont_WhenSetFontCalled()
        {
            // Arrange
            Font actualFont = null;
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            mockPreviewControl
                .Setup(x => x.SetFont(It.IsAny<Font>()))
                .Callback<Font>((font) =>
                {
                    actualFont = font;
                });
            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            var logFont = GetLogFont();

            // Act
            testPreviewHandler.SetFont(ref logFont);

            // Assert
            mockPreviewControl.Verify(_ => _.SetFont(It.IsAny<Font>()), Times.Once);
            Assert.AreEqual(Font.FromLogFont(logFont), actualFont);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldCallPreviewControlSetFocus_WhenSetFocusCalled()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();

            // Act
            testPreviewHandler.SetFocus();

            // Assert
            mockPreviewControl.Verify(_ => _.SetFocus(), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldSetHandleOnQueryFocus_WhenPreviewControlsReturnValidHandle()
        {
            // Arrange
            var hwnd = new IntPtr(5);
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            mockPreviewControl.Setup(x => x.QueryFocus(out hwnd));
            var actualHwnd = IntPtr.Zero;

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();

            // Act
            testPreviewHandler.QueryFocus(out actualHwnd);

            // Assert
            Assert.AreEqual(actualHwnd, hwnd);
            mockPreviewControl.Verify(_ => _.QueryFocus(out hwnd), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldThrowOnQueryFocus_WhenPreviewControlsReturnNotValidHandle()
        {
            // Arrange
            var hwnd = IntPtr.Zero;
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            mockPreviewControl.Setup(x => x.QueryFocus(out hwnd));
            var actualHwnd = IntPtr.Zero;

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            Win32Exception exception = null;

            // Act
            try
            {
                testPreviewHandler.QueryFocus(out actualHwnd);
            }
            catch (Win32Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            mockPreviewControl.Verify(_ => _.QueryFocus(out hwnd), Times.Once);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldDirectKeyStrokesToIPreviewHandlerFrame_IfIPreviewHandlerFrameIsSet()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            var mockPreviewHandlerFrame = new Mock<IPreviewHandlerFrame>();
            var msg = default(MSG);

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            testPreviewHandler.SetSite(mockPreviewHandlerFrame.Object);

            // Act
            testPreviewHandler.TranslateAccelerator(ref msg);

            // Assert
            mockPreviewHandlerFrame.Verify(_ => _.TranslateAccelerator(ref msg), Times.Once);
        }

        [DataTestMethod]
        [DataRow(0U)]
        [DataRow(1U)]
        public void PreviewHandlerBase_ShouldReturnIPreviewHandlerFrameResponse_IfIPreviewHandlerFrameIsSet(uint resultCode)
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            var mockPreviewHandlerFrame = new Mock<IPreviewHandlerFrame>();
            var msg = default(MSG);
            mockPreviewHandlerFrame
                .Setup(x => x.TranslateAccelerator(ref msg))
                .Returns(resultCode);

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            testPreviewHandler.SetSite(mockPreviewHandlerFrame.Object);

            // Act
            var actualResultCode = testPreviewHandler.TranslateAccelerator(ref msg);

            // Assert
            Assert.AreEqual(resultCode, actualResultCode);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldReturnS_FALSE_IfIPreviewHandlerFrameIsNotSet()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            var msg = default(MSG);
            uint sFalse = 1;

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();

            // Act
            var result = testPreviewHandler.TranslateAccelerator(ref msg);

            // Assert
            Assert.AreEqual(result, sFalse);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldReturnPreviewControlHandle_IfGetWindowCalled()
        {
            // Arrange
            var previewControlHandle = new IntPtr(5);
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();
            mockPreviewControl.Setup(x => x.GetWindowHandle())
                .Returns(previewControlHandle);

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            var hwndReceived = IntPtr.Zero;

            // Act
            testPreviewHandler.GetWindow(out hwndReceived);

            // Assert
            Assert.AreEqual(hwndReceived, previewControlHandle);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void PreviewHandlerBase_ShouldThrowNotImplementedException_IfContextSensitiveHelpCalled(bool fEnterMode)
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            NotImplementedException exception = null;

            // Act
            try
            {
                testPreviewHandler.ContextSensitiveHelp(fEnterMode);
            }
            catch (NotImplementedException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void PreviewHandlerBase_ShouldReturnSite_WhenGetSiteCalled()
        {
            // Arrange
            var mockPreviewControl = new Mock<IPreviewHandlerControl>();

            previewHandlerControl = mockPreviewControl.Object;
            var testPreviewHandler = new TestPreviewHandler();
            var site = new Mock<IPreviewHandlerFrame>().Object;
            testPreviewHandler.SetSite(site);
            object actualSite = null;
            var riid = Guid.Empty;

            // Act
            testPreviewHandler.GetSite(ref riid, out actualSite);

            // Assert
            Assert.AreEqual(actualSite, site);
        }

        private LOGFONT GetLogFont()
        {
            var logFont = new LOGFONT
            {
                LfHeight = 12,
                LfWidth = 0,
                LfEscapement = 0,
                LfWeight = 400, // FW_NORMAL
                LfItalic = Convert.ToByte(false),
                LfUnderline = Convert.ToByte(false),
                LfStrikeOut = Convert.ToByte(0),
                LfCharSet = Convert.ToByte(0), // ANSI_CHARSET
                LfOutPrecision = Convert.ToByte(0), // OUT_DEFAULT_PRECIS
                LfClipPrecision = Convert.ToByte(0),
                LfQuality = Convert.ToByte(0),
                LfPitchAndFamily = Convert.ToByte(0),
                LfFaceName = "valid-font",
            };

            return logFont;
        }

        private RECT GetRectangle(int left, int top, int right, int bottom)
        {
            var rect = new RECT
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom,
            };

            return rect;
        }

        private static IPreviewHandlerControl GetMockPreviewHandlerControl()
        {
            return previewHandlerControl;
        }
    }
}
