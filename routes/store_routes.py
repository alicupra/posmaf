from flask import Blueprint, request, jsonify
from models.store import Store, db

store_routes = Blueprint('store_routes', __name__)

@store_routes.route('/stores', methods=['GET'])
def get_stores():
    stores = Store.query.all()
    return jsonify([{"id": store.id, "name": store.name, "location": store.location} for store in stores])

@store_routes.route('/stores', methods=['POST'])
def add_store():
    data = request.json
    store = Store(name=data['name'], location=data['location'])
    db.session.add(store)
    db.session.commit()
    return jsonify({"message": "Store added successfully"}), 201