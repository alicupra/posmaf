from flask_sqlalchemy import SQLAlchemy

db = SQLAlchemy()

class Product(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    name = db.Column(db.String(80), nullable=False)
    price = db.Column(db.Float, nullable=False)
    stock = db.Column(db.Integer, nullable=False)
    threshold = db.Column(db.Integer, nullable=False)
    store_id = db.Column(db.Integer, db.ForeignKey('store.id'), nullable=False)