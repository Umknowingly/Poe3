-- =====================================================
-- BIMO Cybersecurity Bot - MySQL Database Setup
-- Run this file in MySQL Workbench or phpMyAdmin
-- =====================================================

-- Step 1: Create the database
CREATE DATABASE IF NOT EXISTS bimo_bot;

-- Step 2: Use the database
USE bimo_bot;

-- Step 3: Create the tasks table
CREATE TABLE IF NOT EXISTS tasks (
    id          INT           AUTO_INCREMENT PRIMARY KEY,
    title       VARCHAR(200)  NOT NULL,
    description TEXT,
    reminder    VARCHAR(100)  DEFAULT 'No reminder set',
    is_complete TINYINT(1)    DEFAULT 0,
    created_at  DATETIME      DEFAULT CURRENT_TIMESTAMP
);

-- Step 4: (Optional) Add some example tasks to test with
INSERT INTO tasks (title, description, reminder, is_complete) VALUES
('Enable Two-Factor Authentication', 'Set up 2FA on email and banking apps.', '3 days', 0),
('Update all passwords',             'Change passwords to strong unique ones.',  'tomorrow', 0),
('Back up important files',          'Copy files to cloud and external drive.',   '1 week', 0);

-- Step 5: Verify the table was created
SELECT * FROM tasks;

-- =====================================================
-- USEFUL QUERIES (for reference)
-- =====================================================

-- View all tasks:
-- SELECT * FROM tasks;

-- View only incomplete tasks:
-- SELECT * FROM tasks WHERE is_complete = 0;

-- Mark a task as complete:
-- UPDATE tasks SET is_complete = 1 WHERE id = 1;

-- Delete a task:
-- DELETE FROM tasks WHERE id = 1;

-- =====================================================
-- HOW TO CONNECT IN VISUAL STUDIO
-- =====================================================
-- 1. Open Visual Studio (purple)
-- 2. Go to: Tools → NuGet Package Manager → Manage NuGet Packages
-- 3. Click "Browse" tab
-- 4. Search for: MySql.Data
-- 5. Click Install on the first result (by Oracle)
-- 6. Wait for it to finish installing
-- 7. Your connection string in the code should be:
--    "Server=localhost;Database=bimo_bot;Uid=root;Pwd=YourPassword;"
--    (Change YourPassword to your actual MySQL password)
-- =====================================================
