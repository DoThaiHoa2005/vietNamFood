# Custom Dialogs Usage Guide

Quick reference for using custom dialogs in VietnamFoodGuide app.

---

## MessageDialog (Information/Warning/Error)

### Show Information Message
```csharp
MessageDialog.ShowInformation("Your message here", "Title");
```

**Example**:
```csharp
MessageDialog.ShowInformation(
    "Vui lòng đăng nhập để sử dụng tính năng này!", 
    "Thông báo"
);
```

**Visual**: Purple header with ℹ️ icon

---

### Show Warning Message
```csharp
MessageDialog.ShowWarning("Your warning here", "Title");
```

**Example**:
```csharp
MessageDialog.ShowWarning(
    $"{lang["connection_error"]}. {lang["offline_mode"]}", 
    lang["connection_lost"]
);
```

**Visual**: Orange header with ⚠️ icon

---

### Show Error Message
```csharp
MessageDialog.ShowError("Your error here", "Title");
```

**Example**:
```csharp
MessageDialog.ShowError(
    $"Lỗi: {ex.Message}", 
    "Lỗi"
);
```

**Visual**: Red header with ❌ icon

---

### Generic Show Method
```csharp
MessageDialog.Show("Message", "Title", MessageType.Information);
MessageDialog.Show("Message", "Title", MessageType.Warning);
MessageDialog.Show("Message", "Title", MessageType.Error);
```

---

## ConfirmDialog (Yes/No Questions)

### Basic Usage
```csharp
bool result = ConfirmDialog.Show("Your question here", "Title");
if (result) {
    // User clicked "Có" (Yes)
} else {
    // User clicked "Không" (No) or closed dialog
}
```

**Example**:
```csharp
bool confirmed = ConfirmDialog.Show(
    "Bạn có muốn bỏ qua bước quét mã QR không?", 
    "Xác nhận"
);

if (confirmed) {
    // Open MainWindow
    MainWindow mainWindow = new MainWindow();
    mainWindow.Show();
    this.Close();
}
```

**Visual**: Purple header with ❓ icon, two buttons

---

### Custom Button Text
```csharp
bool result = ConfirmDialog.Show(
    "Your question", 
    "Title", 
    yesText: "Đồng ý",  // Custom Yes text
    noText: "Hủy"       // Custom No text
);
```

**Example**:
```csharp
bool deleteConfirmed = ConfirmDialog.Show(
    "Bạn có chắc chắn muốn xóa món ăn này?", 
    "Xác nhận xóa",
    yesText: "Xóa",
    noText: "Hủy"
);
```

---

## Migration from MessageBox

### Before (MessageBox)
```csharp
MessageBox.Show(
    "Message", 
    "Title", 
    MessageBoxButton.OK, 
    MessageBoxImage.Information
);
```

### After (MessageDialog)
```csharp
MessageDialog.ShowInformation("Message", "Title");
```

---

### Before (MessageBox with Yes/No)
```csharp
var result = MessageBox.Show(
    "Question?", 
    "Title", 
    MessageBoxButton.YesNo, 
    MessageBoxImage.Question
);

if (result == MessageBoxResult.Yes) {
    // Do something
}
```

### After (ConfirmDialog)
```csharp
bool result = ConfirmDialog.Show("Question?", "Title");
if (result) {
    // Do something
}
```

---

## Common Patterns

### Error Handling
```csharp
try {
    // Your code
} catch (Exception ex) {
    MessageDialog.ShowError($"Lỗi: {ex.Message}", "Lỗi");
}
```

---

### Login Required
```csharp
if (App.CurrentApiUser == null) {
    MessageDialog.ShowInformation(
        "Vui lòng đăng nhập để sử dụng tính năng này!", 
        "Thông báo"
    );
    return;
}
```

---

### Confirmation Before Action
```csharp
bool confirmed = ConfirmDialog.Show(
    "Bạn có chắc chắn muốn thực hiện hành động này?", 
    "Xác nhận"
);

if (!confirmed) return;

// Proceed with action
```

---

### Network Error
```csharp
MessageDialog.ShowWarning(
    "Không thể kết nối đến server. Đang chuyển sang chế độ offline.", 
    "Mất kết nối"
);
```

---

### Success Message
```csharp
MessageDialog.ShowInformation(
    "Thao tác đã được thực hiện thành công!", 
    "Thành công"
);
```

---

## Best Practices

### 1. Use Appropriate Message Type
- **Information**: General notifications, success messages
- **Warning**: Non-critical issues, offline mode, missing data
- **Error**: Critical errors, exceptions, failed operations

### 2. Keep Messages Concise
```csharp
// Good
MessageDialog.ShowError("Không thể tải dữ liệu", "Lỗi");

// Too verbose
MessageDialog.ShowError(
    "Đã xảy ra lỗi khi cố gắng tải dữ liệu từ server. " +
    "Vui lòng kiểm tra kết nối internet của bạn và thử lại. " +
    "Nếu vấn đề vẫn tiếp diễn, vui lòng liên hệ hỗ trợ kỹ thuật.",
    "Lỗi"
);
```

### 3. Use Language Service for Multilingual Support
```csharp
var lang = LanguageService.Instance;
MessageDialog.ShowError(
    $"{lang["error"]}: {ex.Message}", 
    lang["error"]
);
```

### 4. Provide Context in Error Messages
```csharp
// Good - includes context
MessageDialog.ShowError($"Lỗi mở QR Scanner: {ex.Message}", "Lỗi");

// Less helpful
MessageDialog.ShowError(ex.Message, "Lỗi");
```

### 5. Use Confirmation for Destructive Actions
```csharp
bool confirmed = ConfirmDialog.Show(
    "Bạn có chắc chắn muốn xóa tất cả dữ liệu?", 
    "Xác nhận xóa"
);

if (!confirmed) return;

// Proceed with deletion
```

---

## Dialog Specifications

### MessageDialog
- **Size**: 320x180px
- **Header Colors**:
  - Information: #6C5CE7 (Purple)
  - Warning: #F39C12 (Orange)
  - Error: #E74C3C (Red)
- **Icons**:
  - Information: ℹ️
  - Warning: ⚠️
  - Error: ❌
- **Button**: Single "OK" button (purple)

### ConfirmDialog
- **Size**: 320x200px
- **Header Color**: #6C5CE7 (Purple)
- **Icon**: ❓
- **Buttons**: "Có" (Yes) and "Không" (No)
- **Return**: Boolean (true = Yes, false = No/Close)

---

## Troubleshooting

### Dialog Not Showing
Make sure you have the using statement:
```csharp
using VietnamFoodGuide.Views;
```

### Dialog Behind Other Windows
Dialogs use `ShowDialog()` internally, which should always be modal. If issues persist, check window ownership.

### Custom Button Text Not Working
Make sure you're using the correct overload:
```csharp
ConfirmDialog.Show(message, title, yesText, noText);
```

---

## Examples from Codebase

### QRScannerWindow.xaml.cs
```csharp
bool confirmed = ConfirmDialog.Show(
    "Bạn có muốn bỏ qua bước quét mã QR không?", 
    "Xác nhận"
);

if (confirmed) {
    MainWindow mainWindow = new MainWindow();
    mainWindow.Show();
    this.Close();
}
```

### FoodDetailWindow.xaml.cs
```csharp
if (App.CurrentApiUser == null) {
    MessageDialog.ShowInformation(
        _lang["need_login"], 
        _lang["notification"]
    );
    return;
}
```

### MainWindow.xaml.cs
```csharp
catch (Exception ex) {
    MessageDialog.ShowError(
        $"Lỗi mở QR Scanner: {ex.Message}", 
        "Lỗi"
    );
}
```

---

## Summary

- Use `MessageDialog` for one-way messages (info/warning/error)
- Use `ConfirmDialog` for yes/no questions
- Always provide clear, concise messages
- Use appropriate message types for visual clarity
- Leverage language service for multilingual support

**Remember**: Custom dialogs are compact, beautiful, and consistent with the app's design! 🎨✨
