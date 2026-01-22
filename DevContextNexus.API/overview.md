# Project Context: DevContext Nexus

## 1. บทนำ (Introduction)
โปรเจกต์นี้มีวัตถุประสงค์เพื่อสร้าง **Centralized Context API** สำหรับจัดเก็บและจัดการไฟล์ `.txt` และ `.md` ที่ใช้เป็น "Source of Truth" ของข้อมูลบริบท (Context) ในการพัฒนาซอฟต์แวร์

## 2. ปัญหาที่ต้องการแก้ไข (Pain Points)
* **ข้อมูลกระจัดกระจาย:** ไฟล์ Context ชุดเดียวกันถูก Copy ไว้หลายที่
* **การอัปเดตไม่พร้อมกัน:** เมื่อมีการเปลี่ยน Logic ใน Backend แต่ไฟล์ใน Frontend ไม่เปลี่ยนตาม

## 3. เป้าหมาย (Goals)
* สร้าง API กลางสำหรับ Read/Write ไฟล์ Markdown ผ่าน GitHub
* มี Database สำหรับทำ Indexing
* รองรับการเรียกใช้งานจากทั้ง Frontend และ Backend