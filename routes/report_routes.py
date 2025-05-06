from flask import Blueprint, jsonify
from models.sale import Sale, db
from sqlalchemy import func

report_routes = Blueprint('reports', __name__)

@report_routes.route('/reports/sales', methods=['GET'])
def sales_report():
    daily_sales = db.session.query(
        func.date(Sale.timestamp),
        func.sum(Sale.total_price)
    ).group_by(func.date(Sale.timestamp)).all()

    report = [{"date": str(sale[0]), "total_sales": sale[1]} for sale in daily_sales]
    
    return jsonify(report)

@report_routes.route('/reports/best-sellers', methods=['GET'])
def best_sellers():
    best_sellers = db.session.query(
        Sale.product_id,
        func.sum(Sale.quantity).label('total_sold')
    ).group_by(Sale.product_id).order_by(func.sum(Sale.quantity).desc()).limit(5).all()

    report = [{"product_id": seller[0], "total_sold": seller[1]} for seller in best_sellers]
    
    return jsonify(report)