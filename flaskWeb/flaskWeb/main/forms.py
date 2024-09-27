from flask_wtf import FlaskForm
from wtforms import IntegerField, SubmitField
from wtforms.validators import DataRequired, NumberRange


class LengthForm(FlaskForm):
	length1 = IntegerField("Length1", validators=[DataRequired(), NumberRange(205,305)])
	length2 = IntegerField("Length2", validators=[DataRequired(), NumberRange(455,755)])
	submit = SubmitField("Commit Change")

