function confirmDelete(uniqueId, isDeleteClicked) {
    let deleteSpan = 'deleteSpan_' + uniqueId;
    let confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isDeleteClicked) {
        document.getElementById(deleteSpan).hidden = true;
        document.getElementById(confirmDeleteSpan).hidden = false;
    } else {
        document.getElementById(deleteSpan).hidden = false;
        document.getElementById(confirmDeleteSpan).hidden = true;
    }
}