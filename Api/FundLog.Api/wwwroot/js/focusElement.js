window.focusElement = element => {
  console.log("focusElement");
  console.log(element);
  element.focus();
  element.select();
  element.setSelectionRange(0, element.value.length);
};