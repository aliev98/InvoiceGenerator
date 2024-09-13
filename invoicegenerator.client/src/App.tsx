import { useState } from 'react';
import axios from 'axios';
import './App.css';

interface InvoiceItem {
    description: string;
    quantity: number;
    unitPrice: number;
    total?: number;
}

interface Invoice {
    clientName: string;
    clientEmail: string;
    clientAddress: string;
    items: InvoiceItem[];
}

const App = () => {
    const [newInvoice, setNewInvoice] = useState<Invoice>({
        clientName: '',
        clientEmail: '',
        clientAddress: '',
        items: []
    });

    const [item, setItem] = useState<InvoiceItem>({
        description: '',
        quantity: 1,
        unitPrice: 0
    });

    const [quantity, setQuantity] = useState(1);
    const [downloaded, setDownloaded] = useState(false);
    const quantities = Array.from({ length: 5 }, (_, i) => i + 1);

    const addItem = () => {
        if (item.description === '' || item.quantity === 0 || item.unitPrice === 0) {
            alert('All item fields need to be filled out before adding.')
            return;
        }

        setNewInvoice({
            ...newInvoice,
            items: [...newInvoice.items, item]
        });

        setItem({ description: '', quantity: 1, unitPrice: 0 });
    };

    const downloadInvoicePdf = async () => {

        if (newInvoice.clientName == '' || newInvoice.clientEmail == '' || newInvoice.items.length == 0) {
            alert('Information needs to be entered before downloading the invoice.')
        }
        const totalPrice = newInvoice.items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0);

        const invoiceData = {
            clientName: newInvoice.clientName,
            clientEmail: newInvoice.clientEmail,
            clientAddress: newInvoice.clientAddress,
            totalAmount: totalPrice,
            items: newInvoice.items
        };

        try {
            const response = await axios.post('https://localhost:7223/api/Invoice/download-invoice', invoiceData, {
                responseType: 'blob',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            const url = window.URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', 'Invoice.pdf');
            document.body.appendChild(link);
            link.click();
            link.parentNode?.removeChild(link);
            setDownloaded(true);
        } catch (error) {
            console.error('Error downloading invoice PDF:', error);
        }
    };

    const previewInvoicePdf = async (invoiceData: Invoice) => {
        if (newInvoice.clientName == '' || newInvoice.clientEmail == '' || newInvoice.items.length == 0) {
            alert('Information needs to be entered before previewing the invoice.')
        }
        try {
            const response = await axios.post('https://localhost:7223/api/Invoice/preview-invoice-pdf', invoiceData, {
                responseType: 'blob',
                headers: {
                    'Content-Type': 'application/json',
                },
            });
            
            const blob = new Blob([response.data], { type: 'application/pdf' });
            const blobUrl = window.URL.createObjectURL(blob);
            window.open(blobUrl, '_blank');
        } catch (error) {
            console.error('Error generating invoice PDF:', error);
        }
    };

    const sendEmail = (email: string, subject: string) => {

        if (!downloaded) {
            alert('Invoice has to be downloaded first.')
            return;
        }

        const mailtoLink = `mailto:${email}?subject=${encodeURIComponent(subject)}`

        window.location.href = mailtoLink;
    };

    return (
        <div id="root">
            <div style={{ textAlign: 'center', marginBottom: '20px' }}>
                <h1 style={{fontSize:'2.5rem'} }>TechTools</h1> 
                <h3>Invoice Generator</h3>
            </div>
            <div className="imgContainer">
            <img src="src/assets/invoice.png"></img>
            </div>
            <div className="form-container">
                {/* Client Information Form */}
                <div className="form-section">
                    <h3>Client Information</h3>
                    <input
                        type="text"
                        placeholder="Client Name"
                        value={newInvoice.clientName}
                        onChange={(e) => {
                            setDownloaded(false);
                            setNewInvoice({
                                ...newInvoice,
                                clientName: e.target.value,
                                items: []
                            });
                        }}
                    />

                    <input
                        type="email"
                        placeholder="Client Email"
                        value={newInvoice.clientEmail}
                        onChange={(e) => {
                            setDownloaded(false); 
                            setNewInvoice({
                                ...newInvoice,
                                clientEmail: e.target.value,
                                items: []
                            });
                        }}
                    />
                    <input
                        type="text"
                        placeholder="Client Address"
                        value={newInvoice.clientAddress}
                        onChange={(e) => {
                            setDownloaded(false); 
                            setNewInvoice({
                                ...newInvoice,
                                clientAddress: e.target.value,
                                items: []
                            });
                        }}
                    />
                </div>

                <div className="form-section">
                    <h3>Ordered Tech Items</h3>
                    <input
                        type="text"
                        placeholder="Name of item"
                        value={item.description}
                        onChange={(e) => setItem({ ...item, description: e.target.value })}
                    />
                    <div className="item-info">
                        <input
                            type="number"
                            placeholder="Price"
                            value={item.unitPrice === 0 ? '' : item.unitPrice}
                            onChange={(e) => setItem({ ...item, unitPrice: +e.target.value || 0 })}
                        />

                        <span style={{fontSize:'20px'} }>$</span>
                        <span>x</span>
                        <select
                            value={quantity}
                            onChange={(e) => {
                                const selectedQuantity = Number(e.target.value);
                                setQuantity(selectedQuantity);
                                setItem((prevItem) => ({ ...prevItem, quantity: selectedQuantity }));
                            }}
                        >
                            {quantities.map((num) => (
                                <option key={num} value={num}>{num}</option>
                            ))}
                        </select>
                    </div>
                    <button onClick={addItem}>Add Item</button>
                </div>

                <ul>
                    {newInvoice.items.map((i, index) => (
                        <li key={index}>
                            {i.description} - {i.quantity} x ${i.unitPrice.toFixed(2)} = ${(i.quantity * i.unitPrice).toFixed(2)}
                        </li>
                    ))}
                </ul>

                <div className="button-container">
                    <button onClick={() => previewInvoicePdf(newInvoice)} title="Preview Invoice">

                        <svg xmlns="http://www.w3.org/2000/svg" width="35" height="35" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001q.044.06.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1 1 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0" />
                        </svg>
                    </button>

                    <button onClick={downloadInvoicePdf} title="Download Invoice">
                        <svg xmlns="http://www.w3.org/2000/svg" width="35" height="35" fill="currentColor"  viewBox="0 0 16 16">
                            <path d="M.5 9.9a.5.5 0 0 1 .5.5v2.5a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-2.5a.5.5 0 0 1 1 0v2.5a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2v-2.5a.5.5 0 0 1 .5-.5" />
                            <path d="M7.646 11.854a.5.5 0 0 0 .708 0l3-3a.5.5 0 0 0-.708-.708L8.5 10.293V1.5a.5.5 0 0 0-1 0v8.793L5.354 8.146a.5.5 0 1 0-.708.708z" />
                        </svg>
                    </button>

                    <button onClick={()=>sendEmail(newInvoice.clientEmail, "Invoice") } title="Mail Invoice">
                        <svg xmlns="http://www.w3.org/2000/svg" width="35" height="35" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M0 4a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2zm2-1a1 1 0 0 0-1 1v.217l7 4.2 7-4.2V4a1 1 0 0 0-1-1zm13 2.383-4.708 2.825L15 11.105zm-.034 6.876-5.64-3.471L8 9.583l-1.326-.795-5.64 3.47A1 1 0 0 0 2 13h12a1 1 0 0 0 .966-.741M1 11.105l4.708-2.897L1 5.383z" />
                        </svg>
                    </button>
                </div>
            </div>
        </div>
    );
};

export default App;