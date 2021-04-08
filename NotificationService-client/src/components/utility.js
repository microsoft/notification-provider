import { toast, ToastContainer, Zoom } from 'react-toastify';

export const copyToClipboard = (item, index, event) => {
    const selectedValue = event?.path[0]?.innerText;
    if(selectedValue){
        navigator.clipboard.writeText(selectedValue);
        toast.info('Copied to clipboard!', {
            position: toast.POSITION.TOP_CENTER,
            autoClose: 2000,
            hideProgressBar: true,
            closeOnClick: true,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
            closeButton: false,
            transition: Zoom
            });
    }
}