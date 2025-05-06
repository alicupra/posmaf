from flask import Blueprint, request, jsonify
from models.product import Product, db

# Define the Blueprint
product_routes = Blueprint('product_routes', __name__)

@product_routes.route('/products', methods=['GET'])
def get_products():
    products = Product.query.all()
    return jsonify([{"id": p.id, "name": p.name, "price": p.price, "stock": p.stock} for p in products])

@product_routes.route('/products', methods=['POST'])
def add_product():
    data = request.json
    product = Product(name=data['name'], price=data['price'], stock=data['stock'], threshold=data['threshold'])
    db.session.add(product)
    db.session.commit()
    return jsonify({"message": "Product added successfully"}), 201