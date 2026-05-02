#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
XEM DỮ LIỆU TRONG foods.db
Cách chạy: python view_foods_db.py
"""

import sqlite3
import os
from datetime import datetime

DB_PATH = r"VietnamFoodGuide\bin\Debug\net48\Data\foods.db"

def main():
    print("=" * 80)
    print("📊 XEM DỮ LIỆU FOODS.DB")
    print("=" * 80)
    print()
    
    # Kiểm tra file
    if not os.path.exists(DB_PATH):
        print(f"❌ Không tìm thấy file: {DB_PATH}")
        return
    
    file_size = os.path.getsize(DB_PATH)
    file_time = datetime.fromtimestamp(os.path.getmtime(DB_PATH))
    
    print(f"✅ File tồn tại: {DB_PATH}")
    print(f"📁 Kích thước: {file_size:,} bytes")
    print(f"📅 Lần sửa cuối: {file_time}")
    print()
    
    # Kết nối database
    try:
        conn = sqlite3.connect(DB_PATH)
        cursor = conn.cursor()
        
        print("✅ Kết nối database thành công!")
        print()
        
        # Danh sách tables
        print("📋 DANH SÁCH TABLES:")
        print("-" * 80)
        cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
        for row in cursor.fetchall():
            print(f"  - {row[0]}")
        print()
        
        # Thống kê
        print("📊 THỐNG KÊ:")
        print("-" * 80)
        cursor.execute("SELECT COUNT(*) FROM Foods")
        total = cursor.fetchone()[0]
        print(f"  Tổng quán ăn: {total}")
        print()
        
        # Cấu trúc bảng
        print("🏗️ CẤU TRÚC BẢNG FOODS:")
        print("-" * 80)
        cursor.execute("PRAGMA table_info(Foods)")
        columns = cursor.fetchall()
        
        print(f"{'ID':<5} {'Tên cột':<25} {'Kiểu dữ liệu':<15} {'Not Null':<10}")
        print("-" * 80)
        for col in columns:
            cid, name, type_, notnull, default, pk = col
            not_null = "YES" if notnull else "NO"
            print(f"{cid:<5} {name:<25} {type_:<15} {not_null:<10}")
        print()
        
        # Top 5 quán ăn
        print("🍜 TOP 5 QUÁN ĂN (theo Rating):")
        print("-" * 80)
        
        cursor.execute("""
            SELECT 
                Id, Name, Category, Rating, Radius, Priority, CooldownMinutes
            FROM Foods 
            ORDER BY Rating DESC, Priority DESC 
            LIMIT 5
        """)
        
        print(f"{'ID':<3} {'Tên':<30} {'Loại':<15} {'Rating':<6} {'Radius':<8} {'Priority':<8} {'Cooldown':<10}")
        print("-" * 80)
        
        for row in cursor.fetchall():
            id_, name, category, rating, radius, priority, cooldown = row
            print(f"{id_:<3} {name:<30} {category:<15} {rating:<6} {radius:<8} {priority:<8} {cooldown:<10}")
        print()
        
        # Thống kê theo category
        print("📊 THỐNG KÊ THEO LOẠI:")
        print("-" * 80)
        
        cursor.execute("""
            SELECT 
                Category, 
                COUNT(*) as Total,
                ROUND(AVG(Rating), 1) as AvgRating
            FROM Foods 
            GROUP BY Category
            ORDER BY Total DESC
        """)
        
        print(f"{'Loại':<20} {'Số lượng':<10} {'Rating TB':<15}")
        print("-" * 80)
        
        for row in cursor.fetchall():
            category, total, avg_rating = row
            print(f"{category:<20} {total:<10} {avg_rating:<15}")
        print()
        
        # Chi tiết 1 quán
        print("📋 CHI TIẾT QUÁN ĐẦU TIÊN:")
        print("-" * 80)
        
        cursor.execute("SELECT * FROM Foods ORDER BY Rating DESC LIMIT 1")
        food = cursor.fetchone()
        
        if food:
            col_names = [desc[0] for desc in cursor.description]
            for i, col_name in enumerate(col_names):
                value = food[i]
                if isinstance(value, str) and len(value) > 60:
                    value = value[:60] + "..."
                print(f"  {col_name:<20}: {value}")
        print()
        
        print("=" * 80)
        print("✅ Hoàn tất!")
        print()
        print("💡 Để xem chi tiết hơn, dùng DB Browser for SQLite:")
        print("   https://sqlitebrowser.org/")
        print()
        
        conn.close()
        
    except sqlite3.Error as e:
        print(f"❌ Lỗi database: {e}")
    except Exception as e:
        print(f"❌ Lỗi: {e}")

if __name__ == "__main__":
    main()
