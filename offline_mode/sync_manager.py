import sqlite3
import pymysql
import logging

# SQLite connection setup
def get_sqlite_connection():
    conn = sqlite3.connect("offline_pos.db")
    return conn

# MySQL connection setup
def get_mysql_connection():
    conn = pymysql.connect(
        host="localhost",
        user="mysql_user",
        password="mysql_password",
        database="pos_db"
    )
    return conn

# Sync data from SQLite to MySQL
def sync_to_mysql():
    try:
        sqlite_conn = get_sqlite_connection()
        mysql_conn = get_mysql_connection()

        sqlite_cursor = sqlite_conn.cursor()
        mysql_cursor = mysql_conn.cursor()

        # Fetch unsynced sales from SQLite
        sqlite_cursor.execute("SELECT * FROM sales WHERE synced = 0")
        unsynced_sales = sqlite_cursor.fetchall()

        for sale in unsynced_sales:
            # Insert into MySQL
            query = "INSERT INTO sales (product_id, quantity, total_price, timestamp) VALUES (%s, %s, %s, %s)"
            mysql_cursor.execute(query, sale[1:])

            # Mark as synced in SQLite
            sqlite_cursor.execute("UPDATE sales SET synced = 1 WHERE id = ?", (sale[0],))

        # Commit changes
        mysql_conn.commit()
        sqlite_conn.commit()

    except Exception as e:
        logging.error(f"Sync error: {e}")
    finally:
        sqlite_conn.close()
        mysql_conn.close()

# Sync periodically (e.g., during app startup or via a background job)
if __name__ == "__main__":
    sync_to_mysql()