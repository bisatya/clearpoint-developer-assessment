import axios from 'axios';
import React, { useState, useEffect } from 'react';
import { Button, Table } from 'react-bootstrap'

const TodoItemList = (props) => {
    const [items, setItems] = useState([])
    const [error, setError] = useState('');
  
    useEffect(() => { getItems() }, [props.refetchToken]);

    async function getItems() {
        try {
            const response = await axios.get('/api/todoitems');
            setItems(response.data);
        } catch (error) {
            setError(error.response.data);
        }
        
    }
  
    async function handleMarkAsComplete(item) {
        try {
            await axios.put(`/api/todoitems/${item.id}`, { ...item, isCompleted: true });
            props.onItemUpdated && props.onItemUpdated(item.id);
        } catch (error) {
            setError(error.response.data);
        }
    }
  
    return (
        <>
            <h1>
                Showing {items.length} Item(s){' '}
                <Button variant="primary" className="pull-right" onClick={getItems}>
                    Refresh
                </Button>
            </h1>

            {error && (<small className='text-danger'>{error}</small>)}

            <Table striped bordered hover>
                <thead>
                    <tr>
                        <th>Id</th>
                        <th>Description</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    {items.map((item) => (
                        <tr key={item.id}>
                            <td>{item.id}</td>
                            <td>{item.description}</td>
                            <td>
                                <Button variant="warning" size="sm" onClick={() => handleMarkAsComplete(item)}>
                                    Mark as completed
                                </Button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </>
    )
};

export default TodoItemList;