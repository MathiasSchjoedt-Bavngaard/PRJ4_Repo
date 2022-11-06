import PropTypes from "prop-types";

const button = ({ text, onClick }) => {
  return (
    <button onClick={onClick} >
      {text}
    </button>
  );
};

button.propTypes = {
  text: PropTypes.string.isRequired,
  onClick: PropTypes.func.isRequired,
};

export default button;
