import os

class Config:
    SECRET_KEY = os.getenv('SECRET_KEY', 'your_secret_key')
    SQLALCHEMY_DATABASE_URI = os.getenv('DATABASE_URL', 'mysql+pymysql://user:password@localhost/pos_db')
    SQLALCHEMY_TRACK_MODIFICATIONS = False
    OFFLINE_DB_URI = 'sqlite:///offline_pos.db'