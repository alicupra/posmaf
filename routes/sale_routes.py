from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required
from models.sale import Sale, db
from models.product import Product

sale_routes = Blueprint('sale', __name__)

@sale_routes.route('/sales', methods=['POST'])
@jwt_required()
def create_sale():
    data = request.json
    product = Product.query.get(data['product_id'])

    if not product or product.stock < data['quantity']:
        return jsonify({"message": "Insufficient stock"}), 400

    product.stock -= data['quantity']
    sale = Sale(product_id=product.id, quantity=data['quantity'], total_price=data['quantity'] * product.price)
    db.session.add(sale)
    db.session.commit()

    return jsonify({"message": "Sale processed successfully"}), 201