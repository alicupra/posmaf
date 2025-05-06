from flask import Blueprint, request, jsonify
from models.user import User, db

# Define the Blueprint
auth_routes = Blueprint('auth_routes', __name__)

# Example login endpoint
@auth_routes.route('/login', methods=['POST'])
def login():
    data = request.json
    user = User.query.filter_by(username=data['username']).first()

    if not user or not user.check_password(data['password']):
        return jsonify({"message": "Invalid credentials"}), 401

    # Example response
    return jsonify({"message": "Login successful"}), 200