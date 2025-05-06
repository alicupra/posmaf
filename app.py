from flask import Flask, render_template
from flask_sqlalchemy import SQLAlchemy  # Import SQLAlchemy
from models import db  # Assuming `db` is defined in models.py
from routes.auth_routes import auth_routes
from routes.product_routes import product_routes
from routes.sale_routes import sale_routes
from routes.report_routes import report_routes
from routes.store_routes import store_routes

def create_app():
    app = Flask(__name__)
    app.config['SQLALCHEMY_DATABASE_URI'] = 'mysql+pymysql://user:password@localhost/pos_db'
    app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False

    # Initialize the database with the Flask app
    db.init_app(app)

    # Register blueprints
    app.register_blueprint(auth_routes)
    app.register_blueprint(product_routes)
    app.register_blueprint(sale_routes)
    app.register_blueprint(report_routes)
    app.register_blueprint(store_routes)

    # Define app routes
    @app.route('/')
    def login():
        return render_template('login.html', title="Login - POS")

    @app.route('/dashboard')
    def dashboard():
        return render_template('dashboard.html', title="Dashboard - POS")

    @app.route('/products')
    def products():
        return render_template('products.html', title="Products - POS")

    @app.route('/sales')
    def sales():
        return render_template('sales.html', title="Sales - POS")

    @app.route('/reports')
    def reports():
        return render_template('reports.html', title="Reports - POS")

    @app.route('/employees')
    def employees():
        return render_template('employees.html', title="Employees - POS")

    return app

if __name__ == "__main__":
    app = create_app()
    app.run(debug=True)